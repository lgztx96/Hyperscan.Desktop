using System;
using Hyperscan.Models;

namespace Hyperscan.Core;

public interface IRegexEngine : IDisposable
{
    void Scan(ReadOnlySpan<byte> utf8Bytes, Func<uint, ulong, ulong, uint, int> onMatchEvent);
}

public static class RegexEngineFactory
{
    public static IRegexEngine Create(RegexEngine engine, string pattern, uint flags, object? context = null)
    {
        return engine switch
        {
            RegexEngine.Hyperscan => BlockDatabase.Compile(pattern, flags),
            RegexEngine.Oniguruma => CreateOnigEngine(pattern, flags, context),
            RegexEngine.DotNet => DotNetEngine.Compile(pattern, flags),
            RegexEngine.Pcre2 => Pcre2Engine.Compile(pattern, flags),
            _ => throw new ArgumentOutOfRangeException(nameof(engine))
        };
    }

    private static unsafe OnigEngine CreateOnigEngine(string pattern, uint flags, object? context)
    {
        OnigurumaApi.EnsureInitialized();

        void* encoding;
        void* syntax;

        if (context is OnigurumaContext ctx)
        {
            encoding = ctx.Encoding != null ? ctx.Encoding : OnigurumaApi.Utf8Encoding;
            syntax = ctx.Syntax != null ? ctx.Syntax : OnigurumaApi.GetSyntax("OnigSyntaxOniguruma");
        }
        else
        {
            encoding = OnigurumaApi.Utf8Encoding;
            syntax = OnigurumaApi.GetSyntax("OnigSyntaxOniguruma");
        }

        return OnigEngine.Compile(pattern, flags, encoding, syntax);
    }
}

public sealed unsafe class OnigurumaContext
{
    public void* Encoding { get; init; }
    public void* Syntax { get; init; }
}
