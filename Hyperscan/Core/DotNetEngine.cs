using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Hyperscan.Core;

public sealed class DotNetEngine : IRegexEngine
{
    private readonly Regex regex;

    private DotNetEngine(Regex regex)
    {
        this.regex = regex;
    }

    public static DotNetEngine Compile(string pattern, uint flags)
    {
        var options = (RegexOptions)flags;
        return new DotNetEngine(new Regex(pattern, options));
    }

    public void Scan(ReadOnlySpan<byte> utf8Bytes, Func<uint, ulong, ulong, uint, int> onMatchEvent)
    {
        string text = Encoding.UTF8.GetString(utf8Bytes);
        ScanUtf16(text, onMatchEvent);
    }

    public void ScanUtf16(ReadOnlySpan<char> text, Func<uint, ulong, ulong, uint, int> onMatchEvent)
    {
        foreach (var match in regex.EnumerateMatches(text))
        {
            onMatchEvent(0, (ulong)match.Index, (ulong)(match.Index + match.Length), 0);
        }
    }

    public void Dispose() { }
}
