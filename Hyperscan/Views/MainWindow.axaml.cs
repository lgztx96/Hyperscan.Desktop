using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Notifications;
using Avalonia.Controls.Primitives;
using Avalonia.LogicalTree;
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

    private static readonly Geometry topGeometry = Geometry.Parse("F1 M 16.298828 1.25 C 16.624348 1.25 16.935221 1.316732 17.231445 1.450195 C 17.527668 1.58366 17.788086 1.762695 18.012695 1.987305 C 18.237305 2.211914 18.41634 2.472332 18.549805 2.768555 C 18.683268 3.064779 18.75 3.375652 18.75 3.701172 L 18.75 16.298828 C 18.75 16.62435 18.683268 16.935221 18.549805 17.231445 C 18.41634 17.52767 18.237305 17.788086 18.012695 18.012695 C 17.788086 18.237305 17.527668 18.416342 17.231445 18.549805 C 16.935221 18.683268 16.624348 18.75 16.298828 18.75 L 11.875 18.75 C 11.705729 18.75 11.559244 18.68815 11.435547 18.564453 C 11.311849 18.440756 11.25 18.294271 11.25 18.125 C 11.25 17.955729 11.311849 17.809244 11.435547 17.685547 C 11.559244 17.56185 11.705729 17.5 11.875 17.5 L 16.25 17.5 C 16.425781 17.5 16.588541 17.467447 16.738281 17.402344 C 16.88802 17.33724 17.019855 17.247721 17.133789 17.133789 C 17.247721 17.019857 17.337238 16.888021 17.402344 16.738281 C 17.467447 16.588543 17.5 16.425781 17.5 16.25 L 17.5 10 L 12.451172 10 C 12.11263 10 11.795247 9.931641 11.499023 9.794922 C 11.202799 9.658203 10.94401 9.475912 10.722656 9.248047 C 10.501302 9.020183 10.325521 8.754883 10.195312 8.452148 C 10.065104 8.149414 10 7.832031 10 7.5 L 10 2.5 L 3.75 2.5 C 3.574219 2.5 3.411458 2.532553 3.261719 2.597656 C 3.111979 2.662762 2.980143 2.752279 2.866211 2.866211 C 2.752279 2.980145 2.66276 3.11198 2.597656 3.261719 C 2.532552 3.411459 2.5 3.574219 2.5 3.75 L 2.5 8.125 C 2.5 8.294271 2.438151 8.440756 2.314453 8.564453 C 2.190755 8.688151 2.044271 8.75 1.875 8.75 C 1.705729 8.75 1.559245 8.688151 1.435547 8.564453 C 1.311849 8.440756 1.25 8.294271 1.25 8.125 L 1.25 3.701172 C 1.25 3.375652 1.316732 3.064779 1.450195 2.768555 C 1.583659 2.472332 1.762695 2.211914 1.987305 1.987305 C 2.211914 1.762695 2.472331 1.58366 2.768555 1.450195 C 3.064779 1.316732 3.375651 1.25 3.701172 1.25 Z M 17.5 8.75 L 17.5 3.75 C 17.5 3.58073 17.467447 3.419598 17.402344 3.266602 C 17.337238 3.113607 17.247721 2.980145 17.133789 2.866211 C 17.019855 2.752279 16.886393 2.662762 16.733398 2.597656 C 16.580402 2.532553 16.41927 2.5 16.25 2.5 L 11.25 2.5 L 11.25 7.5 C 11.25 7.675781 11.282552 7.84017 11.347656 7.993164 C 11.41276 8.146159 11.50065 8.277995 11.611328 8.388672 C 11.722005 8.49935 11.853841 8.58724 12.006836 8.652344 C 12.15983 8.717448 12.324219 8.75 12.5 8.75 Z M 8.125 11.25 C 8.294271 11.25 8.440755 11.31185 8.564453 11.435547 C 8.68815 11.559245 8.75 11.705729 8.75 11.875 L 8.75 18.125 C 8.75 18.294271 8.68815 18.440756 8.564453 18.564453 C 8.440755 18.68815 8.294271 18.75 8.125 18.75 C 7.955729 18.75 7.809245 18.68815 7.685547 18.564453 C 7.561849 18.440756 7.5 18.294271 7.5 18.125 L 7.5 13.378906 L 2.314453 18.564453 C 2.190755 18.68815 2.044271 18.75 1.875 18.75 C 1.705729 18.75 1.559245 18.68815 1.435547 18.564453 C 1.311849 18.440756 1.25 18.294271 1.25 18.125 C 1.25 17.955729 1.311849 17.809244 1.435547 17.685547 L 6.621094 12.5 L 1.875 12.5 C 1.705729 12.5 1.559245 12.438151 1.435547 12.314453 C 1.311849 12.190756 1.25 12.044271 1.25 11.875 C 1.25 11.705729 1.311849 11.559245 1.435547 11.435547 C 1.559245 11.31185 1.705729 11.25 1.875 11.25 Z ");
    private static readonly Geometry notTopGeometry = Geometry.Parse("F1 M 16.298828 1.25 C 16.624348 1.25 16.935221 1.316732 17.231445 1.450195 C 17.527668 1.58366 17.788086 1.762695 18.012695 1.987305 C 18.237305 2.211914 18.41634 2.472332 18.549805 2.768555 C 18.683268 3.064779 18.75 3.375652 18.75 3.701172 L 18.75 16.298828 C 18.75 16.62435 18.683268 16.935221 18.549805 17.231445 C 18.41634 17.52767 18.237305 17.788086 18.012695 18.012695 C 17.788086 18.237305 17.527668 18.416342 17.231445 18.549805 C 16.935221 18.683268 16.624348 18.75 16.298828 18.75 L 11.875 18.75 C 11.705729 18.75 11.559244 18.68815 11.435547 18.564453 C 11.311849 18.440756 11.25 18.294271 11.25 18.125 C 11.25 17.955729 11.311849 17.809244 11.435547 17.685547 C 11.559244 17.56185 11.705729 17.5 11.875 17.5 L 16.25 17.5 C 16.425781 17.5 16.588541 17.467447 16.738281 17.402344 C 16.88802 17.33724 17.019855 17.247721 17.133789 17.133789 C 17.247721 17.019857 17.337238 16.888021 17.402344 16.738281 C 17.467447 16.588543 17.5 16.425781 17.5 16.25 L 17.5 10 L 12.451172 10 C 12.11263 10 11.795247 9.931641 11.499023 9.794922 C 11.202799 9.658203 10.94401 9.475912 10.722656 9.248047 C 10.501302 9.020183 10.325521 8.754883 10.195312 8.452148 C 10.065104 8.149414 10 7.832031 10 7.5 L 10 2.5 L 3.75 2.5 C 3.574219 2.5 3.411458 2.532553 3.261719 2.597656 C 3.111979 2.662762 2.980143 2.752279 2.866211 2.866211 C 2.752279 2.980145 2.66276 3.11198 2.597656 3.261719 C 2.532552 3.411459 2.5 3.574219 2.5 3.75 L 2.5 8.125 C 2.5 8.294271 2.438151 8.440756 2.314453 8.564453 C 2.190755 8.688151 2.044271 8.75 1.875 8.75 C 1.705729 8.75 1.559245 8.688151 1.435547 8.564453 C 1.311849 8.440756 1.25 8.294271 1.25 8.125 L 1.25 3.701172 C 1.25 3.375652 1.316732 3.064779 1.450195 2.768555 C 1.583659 2.472332 1.762695 2.211914 1.987305 1.987305 C 2.211914 1.762695 2.472331 1.58366 2.768555 1.450195 C 3.064779 1.316732 3.375651 1.25 3.701172 1.25 Z M 17.5 8.75 L 17.5 3.75 C 17.5 3.58073 17.467447 3.419598 17.402344 3.266602 C 17.337238 3.113607 17.247721 2.980145 17.133789 2.866211 C 17.019855 2.752279 16.886393 2.662762 16.733398 2.597656 C 16.580402 2.532553 16.41927 2.5 16.25 2.5 L 11.25 2.5 L 11.25 7.5 C 11.25 7.675781 11.282552 7.84017 11.347656 7.993164 C 11.41276 8.146159 11.50065 8.277995 11.611328 8.388672 C 11.722005 8.49935 11.853841 8.58724 12.006836 8.652344 C 12.15983 8.717448 12.324219 8.75 12.5 8.75 Z M 8.75 18.125 C 8.75 18.294271 8.68815 18.440756 8.564453 18.564453 C 8.440755 18.68815 8.294271 18.75 8.125 18.75 L 1.875 18.75 C 1.705729 18.75 1.559245 18.68815 1.435547 18.564453 C 1.311849 18.440756 1.25 18.294271 1.25 18.125 L 1.25 11.875 C 1.25 11.705729 1.311849 11.559245 1.435547 11.435547 C 1.559245 11.31185 1.705729 11.25 1.875 11.25 C 2.044271 11.25 2.190755 11.31185 2.314453 11.435547 C 2.438151 11.559245 2.5 11.705729 2.5 11.875 L 2.5 16.621094 L 7.685547 11.435547 C 7.809245 11.31185 7.955729 11.25 8.125 11.25 C 8.294271 11.25 8.440755 11.31185 8.564453 11.435547 C 8.68815 11.559245 8.75 11.705729 8.75 11.875 C 8.75 12.044271 8.68815 12.190756 8.564453 12.314453 L 3.378906 17.5 L 8.125 17.5 C 8.294271 17.5 8.440755 17.56185 8.564453 17.685547 C 8.68815 17.809244 8.75 17.955729 8.75 18.125 Z ");

    private readonly WindowNotificationManager notificationManager;

    private readonly uint[] flags = new uint[1];

    private MainViewModel ViewModel { get; set; }

    [GeneratedRegex(@"\s+")]
    public static partial Regex WhitespaceRegex { get; }

#pragma warning disable CS8618
    public MainWindow()
    {

    }
#pragma warning restore CS8618

    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        ViewModel = viewModel;
        MatchResult.Inlines = [];
        MatchHexResult.Inlines = [];
        notificationManager = new WindowNotificationManager(TopLevel.GetTopLevel(this));

        RenderOptions.SetTextRenderingMode(this, TextRenderingMode.SubpixelAntialias);
        RenderOptions.SetEdgeMode(this, EdgeMode.Antialias);

        viewModel.Source = new FlatTreeDataGridSource<MatchContent>([])
        {
            Columns =
            {
                new TextColumn<MatchContent, uint>("offset", x => x.Offset),
                new TextColumn<MatchContent, string>("content (utf8)", x => x.Content, new GridLength(200), new TextColumnOptions<MatchContent>{ CanUserSortColumn = false }),
                new TextColumn<MatchContent, string>("hex", x => x.Hex, new GridLength(100), new TextColumnOptions<MatchContent>{ CanUserSortColumn = false }),
                new TextColumn<MatchContent, int>("byte length", x => x.ByteLength),
                new TextColumn<MatchContent, string>("", x => null, GridLength.Star, new TextColumnOptions<MatchContent>{ CanUserResizeColumn = false, CanUserSortColumn = false, BeginEditGestures = BeginEditGestures.None }),
            },
        };
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
                    if ((flags[0] & HyperscanApi.HS_FLAG_SOM_LEFTMOST) == HyperscanApi.HS_FLAG_SOM_LEFTMOST)
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
                        }
                    }

                    matchCount++;

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
                    if ((flags[0] & HyperscanApi.HS_FLAG_SOM_LEFTMOST) == HyperscanApi.HS_FLAG_SOM_LEFTMOST)
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
                    ViewModel.Source.Items = null;
                    var matches = new List<MatchContent>();

                    long length = new FileInfo(ViewModel.FilePath).Length;
                    if (length > int.MaxValue)
                    {
                        MatchLargeFile(pattern, ViewModel.FilePath);
                        return;
                    }

                    using MemoryMappedFile mapping = MemoryMappedFile.CreateFromFile(ViewModel.FilePath, FileMode.Open);
                    using MemoryMappedViewAccessor view = mapping.CreateViewAccessor();

                    byte* pointer = null;
                    view.SafeMemoryMappedViewHandle.AcquirePointer(ref pointer);

                    Match(pattern, new ReadOnlySpan<byte>(pointer, (int)length), (id, from, to, _) =>
                    {
                        if ((flags[0] & HyperscanApi.HS_FLAG_SOM_LEFTMOST) == HyperscanApi.HS_FLAG_SOM_LEFTMOST)
                        {
                            var bytes = new ReadOnlySpan<byte>(pointer + (int)from, (int)(to - from));
                            string content = Encoding.UTF8.GetString(bytes);
                            string hex = ToHex(bytes, false);
                            matches.Add(new MatchContent((uint)from, content, hex, bytes.Length));
                        }

                        matchCount++;
                        return 0;
                    });

                    view.SafeMemoryMappedViewHandle.ReleasePointer();
                    ViewModel.Matches = matches;

                    ViewModel.Source.Items = matches;
                    //= new FlatTreeDataGridSource<MatchContent>(matches)
                    //{
                    //    Columns =
                    //    {
                    //        new TextColumn<MatchContent, uint>("offset", x => x.Offset),
                    //        new TextColumn<MatchContent, string>("content (utf8)", x => x.Content, new GridLength(200), new TextColumnOptions<MatchContent>{ CanUserSortColumn = false }),
                    //        new TextColumn<MatchContent, string>("hex", x => x.Hex, new GridLength(100), new TextColumnOptions<MatchContent>{ CanUserSortColumn = false }),
                    //        new TextColumn<MatchContent, int>("byte length", x => x.ByteLength),
                    //        new TextColumn<MatchContent, string>("", x => null, GridLength.Star, new TextColumnOptions<MatchContent>{ CanUserResizeColumn = false, CanUserSortColumn = false, BeginEditGestures = BeginEditGestures.None }),
                    //    },
                    //};

                }
                catch (IOException ex) when (ex.HResult == -2147024888 && !Environment.Is64BitProcess)
                {
                    MatchLargeFile(pattern, ViewModel.FilePath);
                }
                catch (Exception ex)
                {
                    notificationManager.Show(new Notification("error", ex.Message, NotificationType.Error));
                }
            }
        }
    }

    public static string FormatDuration(double microseconds)
    {
        const double usPerMillisecond = 1000;
        const double usPerSecond = 1000 * usPerMillisecond;
        const double usPerMinute = 60 * usPerSecond;
        const double usPerHour = 60 * usPerMinute;
        const double usPerDay = 24 * usPerHour;

        return microseconds switch
        {
            < usPerMillisecond => $"{microseconds:F2} µs ",
            < usPerSecond => $"{microseconds / usPerMillisecond:F2} ms ",
            < usPerMinute => $"{microseconds / usPerSecond:F2} sec ",
            < usPerHour => $"{microseconds / usPerMinute:F2} min ",
            < usPerDay => $"{microseconds / usPerHour:F2} hr ",
            _ => $"{microseconds / usPerDay:F2} day "
        };
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

        this.flags[0] = flags;

        var watch = new System.Diagnostics.Stopwatch();

        try
        {
            using var db = BlockDatabase.Compile(pattern, flags);
            watch.Start();
            db.Scan(text, onMatchEvent);
        }
        catch (ArgumentException ex)
        {
            notificationManager.Show(new Notification("error", ex.Message, NotificationType.Error));
            return;
        }

        watch.Stop();

        double microseconds = watch.Elapsed.TotalMicroseconds;
        string match = matchCount switch
        {
            0 => "no match",
            1 => "1 match",
            _ => $"{matchCount} matches"
        };

        MatchInfo.Text = $"{match}, {FormatDuration(microseconds)}";
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

        this.flags[0] = flags;

        var watch = new System.Diagnostics.Stopwatch();
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
            watch.Start();
            while ((readSize = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                totalSize += readSize;

                db.ScanStream(buffer.AsSpan(0, readSize), (id, from, to, _) =>
                {
                    if ((this.flags[0] & HyperscanApi.HS_FLAG_SOM_LEFTMOST) == HyperscanApi.HS_FLAG_SOM_LEFTMOST)
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
                        matches.Add(new MatchContent((uint)from, content, hex, bytes.Length));
                    }

                    matchCount++;
                    return 0;
                });
            }

            db.CloseStream();
            ViewModel.Matches = matches;
            ViewModel.Source.Items = matches;
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
        double microseconds = watch.Elapsed.TotalMicroseconds;
        string match = matchCount switch
        {
            0 => "no match",
            1 => "1 match",
            _ => $"{matchCount} matches"
        };

        MatchInfo.Text = $"{match}, {FormatDuration(microseconds)}";
    }

    private async void OpenOnClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open File",
            AllowMultiple = false,
            FileTypeFilter = [new FilePickerFileType("All files") { Patterns = ["*"] }]
        });

        if (files.Count != 0 && files[0].TryGetLocalPath() is string path)
        {
            ViewModel.FilePath = path;
        }
    }

    private void OnEnterAlwaysOnTopBthClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var button = sender as ToggleButton;
        var icon = button!.Content as PathIcon;
        if (button.IsChecked is true)
        {
            this.Topmost = true;
            ToolTip.SetTip(button, "Not always on top");
            icon!.Data = notTopGeometry;
        }
        else
        {
            this.Topmost = false;
            ToolTip.SetTip(button, "Always on top");
            icon!.Data = topGeometry;
        }
    }
}
