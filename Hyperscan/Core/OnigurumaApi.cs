
global using onig_error_info_t = Hyperscan.Core.onig_error_info;
global using onig_region_t = Hyperscan.Core.onig_region;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Hyperscan.Core;

public unsafe struct onig_region
{
    public int allocated;
    public int num_regs;
    public int* beg;
    public int* end;
    public void* history_root;
}

public unsafe struct onig_error_info
{
    public void* enc;
    public byte* par;
    public byte* par_end;
}

public static unsafe partial class OnigurumaApi
{
    public const int ONIG_NORMAL = 0;
    public const int ONIG_MISMATCH = -1;

    public const uint ONIG_OPTION_NONE = 0;
    public const uint ONIG_OPTION_IGNORECASE = 1;
    public const uint ONIG_OPTION_EXTEND = 2;
    public const uint ONIG_OPTION_MULTILINE = 4;
    public const uint ONIG_OPTION_SINGLELINE = 8;
    public const uint ONIG_OPTION_FIND_LONGEST = 16;
    public const uint ONIG_OPTION_FIND_NOT_EMPTY = 32;
    public const uint ONIG_OPTION_NEGATE_SINGLELINE = 64;
    public const uint ONIG_OPTION_DONT_CAPTURE_GROUP = 128;
    public const uint ONIG_OPTION_CAPTURE_GROUP = 256;

    public const int ONIG_MAX_ERROR_MESSAGE_LEN = 90;

    private static void* utf8Encoding;
    private static void* utf16LeEncoding;
    private static readonly Dictionary<string, nint> syntaxMap = [];
    private static bool initialized;

    public static void* Utf8Encoding => utf8Encoding;
    public static void* Utf16LeEncoding => utf16LeEncoding;


    public static void EnsureInitialized()
    {
        if (initialized) return;

        nint handle;
        if (RuntimeFeature.IsDynamicCodeCompiled)
        {
            handle = NativeLibrary.Load("onig");
        } 
        else
        {
            handle = NativeLibrary.GetMainProgramHandle();
        }

        nint encPtr = NativeLibrary.GetExport(handle, "OnigEncodingUTF8");
        utf8Encoding = (void*)encPtr;

        try
        {
            nint enc16Ptr = NativeLibrary.GetExport(handle, "OnigEncodingUTF16_LE");
            utf16LeEncoding = (void*)enc16Ptr;
        }
        catch
        {
            utf16LeEncoding = utf8Encoding;
        }

        void* enc = utf8Encoding;
        onig_initialize(&enc, 1);

        string[] syntaxNames =
       [
           "OnigSyntaxOniguruma", "OnigSyntaxRuby", "OnigSyntaxPerl",
            "OnigSyntaxPerl_NG", "OnigSyntaxPython", "OnigSyntaxJava",
            "OnigSyntaxPosixExtended", "OnigSyntaxPosixBasic", "OnigSyntaxGrep",
            "OnigSyntaxEmacs", "OnigSyntaxASIS"
       ];

        foreach (var name in syntaxNames)
        {
            try
            {
                nint ptr = NativeLibrary.GetExport(handle, name);
                syntaxMap[name] = ptr;
            }
            catch
            {
                // syntax not available in this build
            }
        }

        initialized = true;
    }

    public static void* GetSyntax(string name)
    {
        EnsureInitialized();
        return syntaxMap.TryGetValue(name, out var ptr) ? (void*)ptr : null;
    }

    [LibraryImport("onig")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static unsafe partial int onig_initialize(void** encodings, int number_of_encodings);

    [LibraryImport("onig")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static unsafe partial int onig_new(void** reg, byte* pattern, byte* pattern_end, uint option, void* enc, void* syntax, onig_error_info_t* einfo);

    [LibraryImport("onig")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static unsafe partial int onig_search(void* reg, byte* str, byte* end, byte* start, byte* range, onig_region_t* region, uint option);

    [LibraryImport("onig")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static unsafe partial void onig_free(void* reg);

    [LibraryImport("onig")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static unsafe partial onig_region_t* onig_region_new();

    [LibraryImport("onig")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static unsafe partial void onig_region_free(onig_region_t* region, int free_self);

    [LibraryImport("onig")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static unsafe partial void onig_region_clear(onig_region_t* region);

    [LibraryImport("onig")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static unsafe partial int onig_error_code_to_str(byte* s, int err_code, void* einfo);

    [LibraryImport("onig")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static unsafe partial int onig_end();
}
