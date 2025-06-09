using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using Avalonia.Utilities;
using System;
using System.Buffers;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Hyperscan.Controls;

public record HighlightRange
{
    [JsonPropertyName("start")]
    public int Index { get; set; }
    [JsonPropertyName("len")]
    public int Length { get; set; }
    [JsonPropertyName("type")]
    public string Type { get; set; } = null!;
}

[JsonSerializable(typeof(List<HighlightRange>))]
public sealed partial class HighlightContext : JsonSerializerContext { }

public sealed partial class HighlightingTextPresenter : TextPresenter
{

    [LibraryImport("tree-sitter-regex", StringMarshalling = StringMarshalling.Utf16, EntryPoint = "highlight")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]

    public static unsafe partial byte* HighLightRegex([MarshalAs(UnmanagedType.LPWStr)] string code, int len, out int retlen);

    /// <summary>
    /// Invalidates the highlight so the presenter updates
    /// </summary>
    public void InvalidateHighlight()
    {
        InvalidateTextLayout();
    }

    public HighlightingTextPresenter()
    {
        Application.Current!.ActualThemeVariantChanged += (s, e) =>
        {
            styleMap = null; // Reset the style map to rebuild it with the new theme
            InvalidateTextLayout(); // Invalidate the text layout to apply new styles
        };
    }

    private FrozenDictionary<string, GenericTextRunProperties>? styleMap;
    private List<HighlightRange>? lastJson;
    private string? lastText;

    public static FrozenDictionary<string, GenericTextRunProperties> BuildStyleMap(
        Typeface typeface, FontFeatureCollection fontFeatures, double fontSize, IBrush? background)
    {
        return new Dictionary<string, GenericTextRunProperties>
        {
            ["^"] = CreateStyle("RegexCaretColor"),
            ["("] = CreateStyle("RegexParenColor"),
            [")"] = CreateStyle("RegexParenColor"),
            ["["] = CreateStyle("RegexBracketColor"),
            ["]"] = CreateStyle("RegexBracketColor"),
            ["class_character"] = CreateStyle("RegexClassColor"),
            ["-"] = CreateStyle("RegexDashColor"),
            ["|"] = CreateStyle("RegexPipeColor"),
            ["identity_escape"] = CreateStyle("RegexEscapeColor"),
            ["{"] = CreateStyle("RegexBraceColor"),
            ["}"] = CreateStyle("RegexBraceColor"),
            ["decimal_digits"] = CreateStyle("RegexDigitColor"),
            ["end_assertion"] = CreateStyle("RegexEndAssertColor"),
        }.ToFrozenDictionary();

        GenericTextRunProperties CreateStyle(string colorResourceKey)
        {
            var h= Application.Current.TryGetResource(colorResourceKey, Application.Current.ActualThemeVariant, out var color);

            return new GenericTextRunProperties(
                typeface,
                fontFeatures,
                fontSize,
                foregroundBrush: new SolidColorBrush((Color)color),
                backgroundBrush: background);
        }
    }

    /// <summary>
    /// Creates the <see cref="TextLayout"/> used to render the text.
    /// </summary>
    /// <returns>A <see cref="TextLayout"/> object.</returns>
    protected override unsafe TextLayout CreateTextLayout()
    {
        if (Text != lastText)
        {
            if (!string.IsNullOrWhiteSpace(Text))
            {
                byte* utf8Bytes = HighLightRegex(Text, Text.Length, out int retLen);
                var json = new ReadOnlySpan<byte>(utf8Bytes, retLen);
                lastJson = JsonSerializer.Deserialize(json, HighlightContext.Default.ListHighlightRange);
            }

            lastText = Text;
        }

        TextLayout result;

        // Retrieves via reflection to avoid rewriting some code, could override measure though
        Size _constraint = GetPrivateConstraint(this);

        int caretIndex = CaretIndex;
        string? preeditText = PreeditText;
        string? text = GetCombinedText(Text, caretIndex, preeditText);

        var typeface = new Typeface(FontFamily, FontStyle, FontWeight.Bold, FontStretch);
        int selectionStart = SelectionStart;
        int selectionEnd = SelectionEnd;
        int start = Math.Min(selectionStart, selectionEnd);
        int length = Math.Max(selectionStart, selectionEnd) - start;

        styleMap ??= BuildStyleMap(typeface, FontFeatures, FontSize, Background); new Dictionary<string, GenericTextRunProperties>
        {
            ["^"] = new GenericTextRunProperties(typeface, FontFeatures, FontSize, foregroundBrush: new SolidColorBrush(Color.Parse("#E06C75")), backgroundBrush: Background),
           // ["pattern_character"] = new GenericTextRunProperties(typeface, FontFeatures, FontSize, foregroundBrush: new SolidColorBrush(Color.Parse("#DCDCAA")), backgroundBrush: Background),
            ["("] = new GenericTextRunProperties(typeface, FontFeatures, FontSize, foregroundBrush: new SolidColorBrush(Color.Parse("#61AFEF")), backgroundBrush: Background),
            [")"] = new GenericTextRunProperties(typeface, FontFeatures, FontSize, foregroundBrush: new SolidColorBrush(Color.Parse("#61AFEF")), backgroundBrush: Background),
            ["["] = new GenericTextRunProperties(typeface, FontFeatures, FontSize, foregroundBrush: new SolidColorBrush(Color.Parse("#C678DD")), backgroundBrush: Background),
            ["]"] = new GenericTextRunProperties(typeface, FontFeatures, FontSize, foregroundBrush: new SolidColorBrush(Color.Parse("#C678DD")), backgroundBrush: Background),
            ["class_character"] = new GenericTextRunProperties(typeface, FontFeatures, FontSize, foregroundBrush: new SolidColorBrush(Color.Parse("#98C379")), backgroundBrush: Background),
            ["-"] = new GenericTextRunProperties(typeface, FontFeatures, FontSize, foregroundBrush: new SolidColorBrush(Color.Parse("#ABB2BF")), backgroundBrush: Background),
            ["|"] = new GenericTextRunProperties(typeface, FontFeatures, FontSize, foregroundBrush: new SolidColorBrush(Color.Parse("#56B6C2")), backgroundBrush: Background),
            ["identity_escape"] = new GenericTextRunProperties(typeface, FontFeatures, FontSize, foregroundBrush: new SolidColorBrush(Color.Parse("#56B6C2")), backgroundBrush: Background),
            ["{"] = new GenericTextRunProperties(typeface, FontFeatures, FontSize, foregroundBrush: new SolidColorBrush(Color.Parse("#D19A66")), backgroundBrush: Background),
            ["}"] = new GenericTextRunProperties(typeface, FontFeatures, FontSize, foregroundBrush: new SolidColorBrush(Color.Parse("#D19A66")), backgroundBrush: Background),
            ["decimal_digits"] = new GenericTextRunProperties(typeface, FontFeatures, FontSize, foregroundBrush: new SolidColorBrush(Color.Parse("#D19A66")), backgroundBrush: Background),
            ["end_assertion"] = new GenericTextRunProperties(typeface, FontFeatures, FontSize, foregroundBrush: new SolidColorBrush(Color.Parse("#E06C75")), backgroundBrush: Background)
        }.ToFrozenDictionary();

        List<ValueSpan<TextRunProperties>> textStyleOverrides = [];
        IList<ValueSpan<TextRunProperties>> highlightOverrides = Array.Empty<ValueSpan<TextRunProperties>>();

        var foreground = Foreground;

        if (lastJson?.Count > 0)
        {
            highlightOverrides = [];
            //var highlightProperties = new GenericTextRunProperties(typeface, FontFeatures, FontSize, foregroundBrush: highlightBrush, backgroundBrush: Background);
            var defaultProperties = new GenericTextRunProperties(typeface, FontFeatures, FontSize, foregroundBrush: Foreground, backgroundBrush: Background);

            foreach (var range in lastJson)
            {
                var p = styleMap.TryGetValue(range.Type, out var style) ? style : defaultProperties;
                highlightOverrides.Add(new(range.Index, range.Length, p));
            }
            //var range = list[0];
            //highlightOverrides.Add(new(range.start, range.len, highlightProperties));
            //int lastOffset = range.start + range.len;

            //for (int i = 1; i < list.Count; i++)
            //{
            //    range = list[i];
            //    var gapLength = range.start - lastOffset;

            //    if (gapLength > 0)
            //    {
            //        // textStyleOverrides do not fallback when the length ends, so manually inserting
            //        // a fallback between highlighted words is required
            //        highlightOverrides.Add(new(lastOffset, gapLength, defaultProperties));
            //    }

            //    highlightOverrides.Add(new(range.start, range.len, highlightProperties));
            //    lastOffset = range.start + range.len;
            //}
        }

        if (!string.IsNullOrEmpty(preeditText))
        {
            var preeditHighlight = new ValueSpan<TextRunProperties>(caretIndex, preeditText.Length,
                    new GenericTextRunProperties(typeface, FontFeatures, FontSize,
                    foregroundBrush: foreground,
                    textDecorations: TextDecorations.Underline));

            textStyleOverrides = [preeditHighlight];
        }
        else
        {
            if (length > 0 && SelectionForegroundBrush != null)
            {
                textStyleOverrides =
                [
                    new ValueSpan<TextRunProperties>(start, length,
                new GenericTextRunProperties(typeface, FontFeatures, FontSize,
                    foregroundBrush: SelectionForegroundBrush)),
            ];
            }
        }

        textStyleOverrides.AddRange(highlightOverrides);

        if (PasswordChar != default(char) && !RevealPassword)
        {
            result = CreateTextLayoutInternal(_constraint, new string(PasswordChar, text?.Length ?? 0), typeface,
                textStyleOverrides);
        }
        else
        {
            result = CreateTextLayoutInternal(_constraint, text, typeface, textStyleOverrides);
        }

        return result;
    }

    /// <summary>
    /// Creates the <see cref="TextLayout"/> used to render the text.
    /// </summary>
    /// <param name="constraint">The constraint of the text.</param>
    /// <param name="text">The text to format.</param>
    /// <param name="typeface"></param>
    /// <param name="textStyleOverrides"></param>
    /// <returns>A <see cref="TextLayout"/> object.</returns>
    private TextLayout CreateTextLayoutInternal(Size constraint, string? text, Typeface typeface,
        IReadOnlyList<ValueSpan<TextRunProperties>>? textStyleOverrides)
    {
        var foreground = Foreground;
        var maxWidth = MathUtilities.IsZero(constraint.Width) ? double.PositiveInfinity : constraint.Width;
        var maxHeight = MathUtilities.IsZero(constraint.Height) ? double.PositiveInfinity : constraint.Height;

        var textLayout = new TextLayout(text, typeface, FontFeatures, FontSize, foreground, TextAlignment,
            TextWrapping, maxWidth: maxWidth, maxHeight: maxHeight, textStyleOverrides: textStyleOverrides,
            flowDirection: FlowDirection, lineHeight: LineHeight, letterSpacing: LetterSpacing);

        return textLayout;
    }

    private static string? GetCombinedText(string? text, int caretIndex, string? preeditText)
    {
        if (string.IsNullOrEmpty(preeditText))
        {
            return text;
        }

        if (string.IsNullOrEmpty(text))
        {
            return preeditText;
        }

        int totalLength = text.Length + preeditText.Length;
        char[] rentedBuffer = ArrayPool<char>.Shared.Rent(totalLength);

        try
        {
            if (caretIndex > 0)
            {
                text.AsSpan(0, caretIndex).CopyTo(rentedBuffer);
            }

            preeditText.AsSpan().CopyTo(rentedBuffer.AsSpan(caretIndex));
            text.AsSpan(caretIndex).CopyTo(rentedBuffer.AsSpan(caretIndex + preeditText.Length));

            return new string(rentedBuffer, 0, totalLength);
        }
        finally
        {
            ArrayPool<char>.Shared.Return(rentedBuffer);
        }
    }

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_constraint")]
    extern static ref Size GetPrivateConstraint(TextPresenter presenter);
}
