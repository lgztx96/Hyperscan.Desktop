using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Unicode;
using static Hyperscan.Core.OnigurumaApi;

namespace Hyperscan.Core;

public sealed unsafe class OnigEngine : IRegexEngine
{
    private readonly void* regex;
    private readonly void* encoding;

    private OnigEngine(void* reg, void* enc)
    {
        regex = reg;
        encoding = enc;
    }

    private static string GetErrorString(int errCode, onig_error_info_t* einfo)
    {
        Span<byte> buf = stackalloc byte[ONIG_MAX_ERROR_MESSAGE_LEN];
        fixed (byte* pBuf = buf)
        {
            onig_error_code_to_str(pBuf, errCode, einfo);
        }

        int len = 0;
        while (len < buf.Length && buf[len] != 0) len++;
        return Encoding.UTF8.GetString(buf[..len]);
    }

    public static OnigEngine Compile(ReadOnlySpan<char> pattern, uint options, void* encoding, void* syntax)
    {
        EnsureInitialized();

        void* reg = null;
        onig_error_info_t einfo;

        if (encoding == OnigurumaApi.Utf16LeEncoding)
        {
            var span = MemoryMarshal.AsBytes(pattern);
            fixed (byte* pPattern = span)
            {
                int r = onig_new(&reg, pPattern, pPattern + span.Length, options, encoding, syntax, &einfo);
                if (r != ONIG_NORMAL)
                {
                    string err = GetErrorString(r, &einfo);
                    throw new ArgumentException(err);
                }
            }
        }
        else
        {
            Span<byte> patternBytes = pattern.Length < 120
                ? stackalloc byte[pattern.Length * 4]
                : new byte[pattern.Length * 4];

            Utf8.FromUtf16(pattern, patternBytes, out int charsRead, out int bytesWritten);
            fixed (byte* pPattern = patternBytes)
            {
                int r = onig_new(&reg, pPattern, pPattern + bytesWritten, options, encoding, syntax, &einfo);
                if (r != ONIG_NORMAL)
                {
                    string err = GetErrorString(r, &einfo);
                    throw new ArgumentException(err);
                }
            }
        }

        return new OnigEngine(reg, encoding);
    }

    public bool IsUtf16 => encoding == OnigurumaApi.Utf16LeEncoding;

    public void Scan(ReadOnlySpan<byte> utf8Bytes, Func<uint, ulong, ulong, uint, int> onMatchEvent)
    {
        onig_region_t* region = onig_region_new();

        try
        {
            fixed (byte* ptr = utf8Bytes)
            {
                byte* end = ptr + utf8Bytes.Length;
                byte* start = ptr;
                byte* range = end;

                while (start <= end)
                {
                    int r = onig_search(regex, ptr, end, start, range, region, 0);

                    if (r == ONIG_MISMATCH)
                        break;

                    if (r < ONIG_MISMATCH)
                    {
                        string err = GetErrorString(r, null);
                        throw new InvalidOperationException(err);
                    }

                    int from = region->beg[0];
                    int to = region->end[0];

                    onMatchEvent(0, (ulong)from, (ulong)to, 0);

                    if (to == from)
                    {
                        start = ptr + to + 1;
                    }
                    else
                    {
                        start = ptr + to;
                    }

                    onig_region_clear(region);
                }
            }
        }
        finally
        {
            onig_region_free(region, 1);
        }
    }

    public void ScanUtf16(ReadOnlySpan<char> chars, Func<uint, ulong, ulong, uint, int> onMatchEvent)
    {
        onig_region_t* region = onig_region_new();

        try
        {
            fixed (char* pChars = chars)
            {
                byte* ptr = (byte*)pChars;
                int byteLen = chars.Length * 2;
                byte* end = ptr + byteLen;
                byte* start = ptr;
                byte* range = end;

                while (start <= end)
                {
                    int r = onig_search(regex, ptr, end, start, range, region, 0);

                    if (r == ONIG_MISMATCH)
                        break;

                    if (r < ONIG_MISMATCH)
                    {
                        string err = GetErrorString(r, null);
                        throw new InvalidOperationException(err);
                    }

                    int from = region->beg[0];
                    int to = region->end[0];

                    onMatchEvent(0, (ulong)from, (ulong)to, 0);

                    if (to == from)
                    {
                        start = ptr + to + 2;
                    }
                    else
                    {
                        start = ptr + to;
                    }

                    onig_region_clear(region);
                }
            }
        }
        finally
        {
            onig_region_free(region, 1);
        }
    }

    public void Dispose()
    {
        onig_free(regex);
    }
}
