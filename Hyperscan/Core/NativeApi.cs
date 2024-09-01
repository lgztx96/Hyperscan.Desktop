
global using hs_compile_error_t = Hyperscan.Core.hs_compile_error;
global using hs_platform_info_t = Hyperscan.Core.hs_platform_info;
global using unsafe match_event_handler = delegate* unmanaged[Cdecl]<uint, ulong, ulong, uint, void*, int>;
using hs_error_t = int;

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Hyperscan.Core;

public unsafe struct hs_compile_error
{
    public byte* message;

    public int expression;
}

public struct hs_platform_info
{
    public uint tune;

    public ulong cpu_features;

    public ulong reserved1;

    public ulong reserved2;
}

public static partial class HyperscanApi
{
    public const uint HS_MODE_BLOCK = 1;

    public const uint HS_MODE_STREAM = 2;

    public const uint HS_SUCCESS = 0;

    public const uint HS_FLAG_CASELESS = 1;

    public const uint HS_FLAG_DOTALL = 2;

    public const uint HS_FLAG_MULTILINE = 4;

    public const uint HS_FLAG_SINGLEMATCH = 8;

    public const uint HS_FLAG_ALLOWEMPTY = 16;

    public const uint HS_FLAG_UTF8 = 32;

    public const uint HS_FLAG_UCP = 64;

    public const uint HS_FLAG_PREFILTER = 128;

    public const uint HS_FLAG_SOM_LEFTMOST = 256;

    public const uint HS_FLAG_COMBINATION = 512;

    public const uint HS_MODE_SOM_HORIZON_LARGE = (1U << 24);

    [LibraryImport("hs", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static unsafe partial hs_error_t hs_compile(string expression, uint flags, uint mode, hs_platform_info_t* platform, void** db, hs_compile_error_t** error);

    [LibraryImport("hs")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static unsafe partial hs_error_t hs_alloc_scratch(void* db, void** scratch);

    [LibraryImport("hs")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static unsafe partial hs_error_t hs_scan(void* db, byte* data, uint length, uint flags, void* scratch, match_event_handler onEvent, void* context);

    [LibraryImport("hs")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static unsafe partial hs_error_t hs_open_stream(void* db, uint flags, void** stream);

    [LibraryImport("hs")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static unsafe partial hs_error_t hs_scan_stream(void* id, byte* data, uint length, uint flags, void* scratch, match_event_handler onEvent, void* ctxt);

    [LibraryImport("hs")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static unsafe partial hs_error_t hs_close_stream(void* id, void* scratch, match_event_handler onEvent, void* ctxt);

    [LibraryImport("hs")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static unsafe partial hs_error_t hs_free_scratch(void* scratch);

    [LibraryImport("hs")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static unsafe partial hs_error_t hs_free_database(void* db);

    [LibraryImport("hs")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static unsafe partial hs_error_t hs_free_compile_error(hs_compile_error_t* error);
}