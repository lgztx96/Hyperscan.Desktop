using System;
using System.Text;
using static Hyperscan.Core.Pcre2Api;

namespace Hyperscan.Core;

public sealed unsafe class Pcre2Engine : IRegexEngine
{
    private readonly void* code;
    private readonly void* matchData;
    private readonly bool jitAvailable;

    private Pcre2Engine(void* code, void* matchData, bool jitAvailable)
    {
        this.code = code;
        this.matchData = matchData;
        this.jitAvailable = jitAvailable;
    }

    private static string GetErrorMessage(int errorcode)
    {
        Span<byte> buf = stackalloc byte[256];
        fixed (byte* pBuf = buf)
        {
            int len = pcre2_get_error_message_8(errorcode, pBuf, (nuint)buf.Length);
            return len > 0 ? Encoding.UTF8.GetString(buf[..len]) : $"PCRE2 error {errorcode}";
        }
    }

    public static Pcre2Engine Compile(string pattern, uint flags)
    {
        byte[] patternBytes = Encoding.UTF8.GetBytes(pattern);

        fixed (byte* pPattern = patternBytes)
        {
            int errorcode = 0;
            nuint erroroffset = 0;

            void* code = pcre2_compile_8(pPattern, (nuint)patternBytes.Length, flags, &errorcode, &erroroffset, null);

            if (code == null)
            {
                string err = GetErrorMessage(errorcode);
                throw new ArgumentException($"{err} (at offset {erroroffset})");
            }

            bool jitAvailable = pcre2_jit_compile_8(code, PCRE2_JIT_COMPLETE) == 0;

            void* matchData = pcre2_match_data_create_from_pattern_8(code, null);
            return new Pcre2Engine(code, matchData, jitAvailable);
        }
    }

    public void Scan(ReadOnlySpan<byte> utf8Bytes, Func<uint, ulong, ulong, uint, int> onMatchEvent)
    {
        fixed (byte* ptr = utf8Bytes)
        {
            nuint startOffset = 0;
            nuint length = (nuint)utf8Bytes.Length;

            while (startOffset <= length)
            {
                int rc = jitAvailable
                    ? pcre2_jit_match_8(code, ptr, length, startOffset, PCRE2_NO_UTF_CHECK, matchData, null)
                    : pcre2_match_8(code, ptr, length, startOffset, PCRE2_NO_UTF_CHECK, matchData, null);

                if (rc == PCRE2_ERROR_NOMATCH)
                    break;

                if (rc < 0)
                {
                    string err = GetErrorMessage(rc);
                    throw new InvalidOperationException(err);
                }

                nuint* ovector = pcre2_get_ovector_pointer_8(matchData);
                nuint from = ovector[0];
                nuint to = ovector[1];

                onMatchEvent(0, from, to, 0);

                if (to == from)
                {
                    startOffset = to + 1;
                }
                else
                {
                    startOffset = to;
                }
            }
        }
    }

    public void Dispose()
    {
        pcre2_match_data_free_8(matchData);
        pcre2_code_free_8(code);
    }
}
