using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Hyperscan.Core;

public static unsafe partial class Pcre2Api
{
    public const int PCRE2_ERROR_NOMATCH = -1;

    public const uint PCRE2_CASELESS = 0x00000008u;
    public const uint PCRE2_DOTALL = 0x00000020u;
    public const uint PCRE2_MULTILINE = 0x00000400u;
    public const uint PCRE2_UCP = 0x00020000u;
    public const uint PCRE2_UTF = 0x00080000u;
    public const uint PCRE2_EXTENDED = 0x00000080u;
    public const uint PCRE2_DUPNAMES = 0x00000040u;
    public const uint PCRE2_NO_AUTO_CAPTURE = 0x00002000u;
    public const uint PCRE2_UNGREEDY = 0x00040000u;
    public const uint PCRE2_ALT_BSUX = 0x00000002u;
    public const uint PCRE2_DOLLAR_ENDONLY = 0x00000010u;
    public const uint PCRE2_FIRSTLINE = 0x00000100u;
    public const uint PCRE2_LITERAL = 0x02000000u;
    public const uint PCRE2_MATCH_INVALID_UTF = 0x04000000u;
    public const uint PCRE2_ANCHORED = 0x80000000u;

    public const uint PCRE2_NO_UTF_CHECK = 0x40000000u;
    public const uint PCRE2_NOTBOL = 0x00000001u;
    public const uint PCRE2_NOTEOL = 0x00000002u;
    public const uint PCRE2_NOTEMPTY = 0x00000004u;
    public const uint PCRE2_NOTEMPTY_ATSTART = 0x00000008u;

    public const int PCRE2_ERROR_NOMEMORY = -34;
    public const int PCRE2_ERROR_BADMAGIC = -35;
    public const int PCRE2_ERROR_JIT_BADOPTION = -50;

    public const uint PCRE2_JIT_COMPLETE = 0x00000001u;

    [LibraryImport("pcre2-8")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static unsafe partial void* pcre2_compile_8(byte* pattern, nuint length, uint options, int* errorcode, nuint* erroroffset, void* context);

    [LibraryImport("pcre2-8")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static unsafe partial void pcre2_code_free_8(void* code);

    [LibraryImport("pcre2-8")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static unsafe partial void* pcre2_match_data_create_from_pattern_8(void* code, void* gcontext);

    [LibraryImport("pcre2-8")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static unsafe partial void pcre2_match_data_free_8(void* match_data);

    [LibraryImport("pcre2-8")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static unsafe partial int pcre2_match_8(void* code, byte* subject, nuint length, nuint startoffset, uint options, void* match_data, void* mcontext);

    [LibraryImport("pcre2-8")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static unsafe partial nuint* pcre2_get_ovector_pointer_8(void* match_data);

    [LibraryImport("pcre2-8")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static unsafe partial nuint pcre2_get_ovector_count_8(void* match_data);

    [LibraryImport("pcre2-8")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static unsafe partial int pcre2_get_error_message_8(int errorcode, byte* buffer, nuint bufflen);

    [LibraryImport("pcre2-8")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static unsafe partial int pcre2_jit_compile_8(void* code, uint options);

    [LibraryImport("pcre2-8")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static unsafe partial int pcre2_jit_match_8(void* code, byte* subject, nuint length, nuint startoffset, uint options, void* match_data, void* mcontext);
}
