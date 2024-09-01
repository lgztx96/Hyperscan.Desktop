using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

using static Hyperscan.Core.HyperscanApi;

namespace Hyperscan.Core;

public sealed unsafe class StreamingDatabase : IDisposable
{
    private readonly void* datebase;
    private readonly void* stream;
    private readonly void* scratch;

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

    private StreamingDatabase(void* db)
    {
        datebase = db;
    }

    private static unsafe string Utf8ByteToString(byte* bytePtr)
    {
        byte* b;
        for (b = bytePtr; *b != 0; ++b) ;
        return Encoding.UTF8.GetString(bytePtr, (int)(b - bytePtr));
    }

    public static StreamingDatabase Compile(string pattern, uint flags)
    {
        void* database = null;
        hs_compile_error_t* compile_err = null;

        if (hs_compile(pattern,
            HS_FLAG_SOM_LEFTMOST | flags,
            HS_MODE_STREAM | HS_MODE_SOM_HORIZON_LARGE,
            null,
            &database,
            &compile_err) != HS_SUCCESS)
        {
            string err = Utf8ByteToString(compile_err->message);
            hs_free_compile_error(compile_err);
            throw new ArgumentException(err);
        }

        return new StreamingDatabase(database);
    }

    public bool OpenStream()
    {
        fixed (void** addressOfstream = &stream)
        fixed (void** addressOfscratch = &scratch)
        {
            hs_alloc_scratch(datebase, addressOfscratch);
            return hs_open_stream(datebase, 0, addressOfstream) == HS_SUCCESS;
        }
    }

    public void CloseStream()
    {
        hs_close_stream(stream, scratch, null, null);
        hs_free_scratch(scratch);
    }

    public unsafe bool ScanStream(ReadOnlySpan<byte> utf8Bytes, Func<uint, ulong, ulong, uint, int> onMatchEvent)
    {
        fixed (byte* ptr = utf8Bytes)
        {
#pragma warning disable CS8500
            if (hs_scan_stream(stream, ptr, (uint)utf8Bytes.Length, 0, scratch, eventHander, &onMatchEvent) != HS_SUCCESS)
            {
                return false;
            }
#pragma warning restore CS8500
        }


        return true;
    }

    public void Dispose()
    {
        hs_free_database(datebase);
    }
}
