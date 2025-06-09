using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using static Hyperscan.Core.HyperscanApi;

namespace Hyperscan.Core;

public sealed unsafe class BlockDatabase : IDisposable
{
    private readonly void* datebase;

    private static readonly match_event_handler eventHander = &EventHandler;

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe int EventHandler(uint id, ulong from, ulong to, uint flags, void* ctx)
    {
        if (ctx != null)
        {
#pragma warning disable CS8500
            return ((Func<uint, ulong, ulong, uint, int>*)ctx)->Invoke(id, from, to, flags);
#pragma warning restore CS8500
        }

        return 0;
    }

    private BlockDatabase(void* db)
    {
        datebase = db;
    }

    private static unsafe string Utf8ByteToString(byte* bytePtr)
    {
        byte* b;
        for (b = bytePtr; *b != 0; ++b);
        return Encoding.UTF8.GetString(bytePtr, (int)(b - bytePtr));
    }

    public static BlockDatabase Compile(string pattern, uint flags)
    {
        void* database = null;
        hs_compile_error_t* compile_err = null;

        if (hs_compile(pattern,
            flags,
            HS_MODE_BLOCK,
            null,
            &database,
            &compile_err) != HS_SUCCESS)
        {
            string err = Utf8ByteToString(compile_err->message);
            hs_free_compile_error(compile_err);
            throw new ArgumentException(err);
        }

        return new BlockDatabase(database);
    }

    public static BlockDatabase Compile(ReadOnlySpan<string> patterns, ReadOnlySpan<uint> flags)
    {
        void* database = null;
        hs_compile_error_t* compile_err = null;

        Span<uint> ids = stackalloc uint[patterns.Length];
        for (int i = 0; i < patterns.Length; i++)
        {
            ids[i] = (uint)i;
        }

        if (hs_compile_multi(patterns,
            flags,
            ids,
             (uint)patterns.Length,
            HS_MODE_BLOCK,
            null,
            &database,
            &compile_err) != HS_SUCCESS)
        {
            string err = Utf8ByteToString(compile_err->message);
            hs_free_compile_error(compile_err);
            throw new ArgumentException(err);
        }

        return new BlockDatabase(database);
    }

    public unsafe bool Scan(ReadOnlySpan<byte> utf8Bytes, Func<uint, ulong, ulong, uint, int> onMatchEvent)
    {
        void* scratch = null;

        try
        {
            hs_alloc_scratch(datebase, &scratch);
            fixed (byte* ptr = utf8Bytes)
            {
#pragma warning disable CS8500
                if (hs_scan(datebase, ptr, (uint)utf8Bytes.Length, 0, scratch, eventHander, &onMatchEvent)
                    != HS_SUCCESS)
                {
                    return false;
                }
#pragma warning restore CS8500
            }
        }
        finally
        {
            hs_free_scratch(scratch);
        }

        return true;
    }

    public void Dispose()
    {
        hs_free_database(datebase);
    }
}
