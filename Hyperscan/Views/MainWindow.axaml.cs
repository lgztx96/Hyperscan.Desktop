using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Notifications;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Hyperscan.Core;
using Hyperscan.Models;
using Hyperscan.ViewModels;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Text;
using System.Text.RegularExpressions;

namespace Hyperscan.Views;

public partial class MainWindow : Window
{
    private byte[]? utf8Text;
    private int offset = 0;
    private int matchCount = 0;

    private static readonly SolidColorBrush SelectColor = new(Color.Parse("#76d176"));

    private readonly WindowNotificationManager notificationManager;

    private MainViewModel ViewModel { get; set; }

    [GeneratedRegex(@"\s+")]
    public static partial Regex WhitespaceRegex { get; }

#pragma warning disable CS8618
    public MainWindow() { }
#pragma warning restore CS8618

    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        ViewModel = viewModel;
        MatchResult.Inlines = [];
        MatchHexResult.Inlines = [];
        notificationManager = new WindowNotificationManager(TopLevel.GetTopLevel(this));
    }

    void Initialize()
    {
        MatchResult.Inlines?.Clear();
        MatchHexResult.Inlines?.Clear();

        MatchInfo.Text = null;

        if (MainTab.SelectedIndex != 0)
        {
            MatchText.Text = null;
        }

        if (MainTab.SelectedIndex != 1)
        {
            MatchHexText.Text = null;
        }

        offset = 0;
        matchCount = 0;
    }

    private unsafe void MatchOnClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (MainTab.SelectedIndex == 0)
        {
            if (PatternTextBox.Text is string pattern && MatchText.Text is string text)
            {
                utf8Text = Encoding.UTF8.GetBytes(text);
                Match(PatternTextBox.Text, utf8Text, (id, from, to, _) =>
                {
                    if (offset < (int)from && offset != (int)from - 1)
                    {
                        MatchResult.Inlines!.Add(new Run { Text = Encoding.UTF8.GetString(utf8Text.AsSpan(offset, (int)from - offset)) });
                    }

                    if (offset <= (uint)from)
                    {
                        MatchResult.Inlines!.Add(new Run
                        {
                            Text = Encoding.UTF8.GetString(utf8Text.AsSpan((int)from, (int)(to - from))),
                            Background = SelectColor
                        });

                        offset = (int)to;
                        matchCount++;
                    }

                    return 0;
                });

                if (offset != utf8Text.Length)
                {
                    MatchResult.Inlines!.Add(new Run { Text = Encoding.UTF8.GetString(utf8Text.AsSpan(offset)) });
                }
            }
        }
        else if (MainTab.SelectedIndex == 1)
        {
            if (PatternTextBox.Text is string pattern && MatchHexText.Text is string text)
            {
                try
                {
                    string hex = WhitespaceRegex.Replace(text, "");
                    utf8Text = Convert.FromHexString(hex);
                }
                catch (FormatException ex)
                {
                    notificationManager.Show(new Notification("error", ex.Message, NotificationType.Error));
                    return;
                }

                Match(pattern, utf8Text, (id, from, to, _) =>
                {
                    if (offset < (int)from && offset != (int)from - 1)
                    {
                        MatchHexResult.Inlines!.Add(new Run { Text = ToHex(utf8Text.AsSpan(offset, (int)from - offset), true) });
                    }

                    if (offset <= (uint)from)
                    {
                        MatchHexResult.Inlines!.Add(new Run
                        {
                            Text = ToHex(utf8Text.AsSpan((int)from, (int)(to - from)), true),
                            Background = SelectColor
                        });

                        offset = (int)to;
                    }
                   
                    matchCount++;

                    return 0;
                });

                if (offset != utf8Text!.Length)
                {
                    MatchHexResult.Inlines!.Add(new Run { Text = ToHex(utf8Text.AsSpan(offset), false) });
                }
            }
        }
        else
        {
            if (PatternTextBox.Text is string pattern && File.Exists(ViewModel.FilePath))
            {
                try
                {
                    var matches = new List<MatchContent>();

                    long length = new FileInfo(ViewModel.FilePath).Length;
                    if (length > int.MaxValue) 
                    {
                        throw new Exception("file size to long");
                    }

                    using MemoryMappedFile mapping = MemoryMappedFile.CreateFromFile(ViewModel.FilePath, FileMode.Open);
                    using MemoryMappedViewAccessor view = mapping.CreateViewAccessor();

                    byte* pointer = null;
                    view.SafeMemoryMappedViewHandle.AcquirePointer(ref pointer);

                    Match(pattern, new ReadOnlySpan<byte>(pointer, (int)length), (id, from, to, _) =>
                    {
                        var bytes = new ReadOnlySpan<byte>(pointer + (int)from, (int)(to - from));
                        string content = Encoding.UTF8.GetString(bytes);
                        string hex = ToHex(bytes, false);
                        matches.Add(new MatchContent((uint)from, content, hex));
                        matchCount++;
                        return 0;
                    });

                    view.SafeMemoryMappedViewHandle.ReleasePointer();
                    ViewModel.Matches = [.. matches];

                }
                catch (IOException)
                {
                    if (!Environment.Is64BitProcess)
                    {
                        MatchLargeFile(pattern, ViewModel.FilePath);
                    }
                }
                catch (Exception ex)
                {
                    notificationManager.Show(new Notification("error", ex.Message, NotificationType.Error));
                }
            }
        }
    }

    private static string ToHex(ReadOnlySpan<byte> bytes, bool suffix)
    {
        var sb = new StringBuilder(bytes.Length * 3);
        foreach (byte b in bytes)
        {
            sb.AppendFormat("{0:x2}", b);
            sb.Append(' ');
        }

        return suffix ? sb.ToString() : sb.ToString(0, sb.Length - 1);
    }

    void Match(string pattern, ReadOnlySpan<byte> text, Func<uint, ulong, ulong, uint, int> onMatchEvent)
    {
        Initialize();

        uint flags = 0;
        foreach (var flag in ViewModel.Flags)
        {
            if (flag.IsSelected)
            {
                flags |= flag.Value;
            }
        }

        var watch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            using var db = BlockDatabase.Compile(pattern, flags);
            db.Scan(text, onMatchEvent);
        }
        catch (ArgumentException ex)
        {
            notificationManager.Show(new Notification("error", ex.Message, NotificationType.Error));
            return;
        }

        watch.Stop();

        double elapsedMs = watch.Elapsed.TotalMilliseconds;
        string count = matchCount > 0 ? matchCount.ToString() : "no";

        MatchInfo.Text = $"{count} matches, {elapsedMs} ms";
    }

    void MatchLargeFile(string pattern, string file)
    {
        Initialize();

        uint flags = 0;
        foreach (var flag in ViewModel.Flags)
        {
            if (flag.IsSelected)
            {
                flags |= flag.Value;
            }
        }

        var watch = System.Diagnostics.Stopwatch.StartNew();
        byte[]? buffer = null;
        try
        {
            using var db = StreamingDatabase.Compile(pattern, flags);
            db.OpenStream();

            using var stream = File.OpenRead(file);

            buffer = ArrayPool<byte>.Shared.Rent(65535);
            int readSize = 0;
            int totalSize = 0;
            var matches = new List<MatchContent>();
            while ((readSize = stream.Read(buffer, 0, buffer.Length)) > 0) 
            {
                totalSize += readSize;

                db.ScanStream(buffer.AsSpan(0, readSize), (id, from, to, _) =>
                {
                    int offset = (int)from - (totalSize - readSize);
                    int length = (int)(to - from);
                    Span<byte> bytes = [];
                    if (offset < 0)
                    {
                        bytes = new byte[length];
                        RandomAccess.Read(stream.SafeFileHandle, bytes[..Math.Abs(offset)], (long)from);
                    }

                    if (!bytes.IsEmpty)
                    {
                        buffer.AsSpan(0, length - Math.Abs(offset)).CopyTo(bytes[Math.Abs(offset)..]);
                    } 
                    else
                    {
                        bytes = buffer.AsSpan(offset, length);
                    }

                    string content = Encoding.UTF8.GetString(bytes);
                    string hex = ToHex(bytes, false);
                    matches.Add(new MatchContent((uint)from, content, hex));
                    matchCount++;
                    return 0;
                });
            }

            db.CloseStream();
            ViewModel.Matches = [.. matches];
        }
        catch (ArgumentException ex)
        {
            notificationManager.Show(new Notification("error", ex.Message, NotificationType.Error));
            return;
        } 
        finally
        {
           if (buffer != null) ArrayPool<byte>.Shared.Return(buffer);
        }

        watch.Stop();
        double elapsedMs = watch.Elapsed.TotalMilliseconds;
        string count = matchCount > 0 ? matchCount.ToString() : "no";
        
        MatchInfo.Text = $"{count} matches, {elapsedMs} ms";
    }

    private async void OpenOnClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open File",
            AllowMultiple = false,
            FileTypeFilter =
            [new FilePickerFileType("All files") { Patterns = ["*"] }]
        });

        if (files.Count != 0 && files[0].TryGetLocalPath() is string path)
        {
            ViewModel.FilePath = path;
        }
    }
}
