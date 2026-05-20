using CommunityToolkit.Mvvm.ComponentModel;
using Hyperscan.Models;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using static Hyperscan.Core.HyperscanApi;
using static Hyperscan.Core.OnigurumaApi;
using static Hyperscan.Core.Pcre2Api;

namespace Hyperscan.ViewModels;

public sealed partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    public partial List<MatchContent>? Matches { get; set; }

    public List<RegexFlag> HsFlags { get; }
    public List<RegexFlag> OnigFlags { get; }
    public List<RegexFlag> DotNetFlags { get; }
    public List<RegexFlag> Pcre2Flags { get; }
    public List<OnigSyntaxOption> OnigSyntaxOptions { get; }
    public List<OnigEncodingOption> OnigEncodingOptions { get; }

    [ObservableProperty]
    public partial string? FilePath { get; set; }

    [ObservableProperty]
    public partial RegexEngine SelectedEngine { get; set; } = RegexEngine.Hyperscan;

    [ObservableProperty]
    public partial OnigSyntaxOption? SelectedOnigSyntax { get; set; }

    [ObservableProperty]
    public partial OnigEncodingOption? SelectedOnigEncoding { get; set; }

    public bool IsOniguruma => SelectedEngine == RegexEngine.Oniguruma;

    public bool IsDotNet => SelectedEngine == RegexEngine.DotNet;

    public bool IsPcre2 => SelectedEngine == RegexEngine.Pcre2;

    public List<RegexFlag> CurrentFlags => SelectedEngine switch
    {
        RegexEngine.Oniguruma => OnigFlags,
        RegexEngine.DotNet => DotNetFlags,
        RegexEngine.Pcre2 => Pcre2Flags,
        _ => HsFlags
    };

    partial void OnSelectedEngineChanged(RegexEngine value)
    {
        OnPropertyChanged(nameof(IsOniguruma));
        OnPropertyChanged(nameof(IsDotNet));
        OnPropertyChanged(nameof(IsPcre2));
        OnPropertyChanged(nameof(CurrentFlags));
    }

    public MainViewModel()
    {
        HsFlags =
        [
            new RegexFlag("HS_FLAG_SOM_LEFTMOST", "Enable leftmost start of match reporting.", HS_FLAG_SOM_LEFTMOST) { IsSelected = true },
            new RegexFlag("HS_FLAG_CASELESS", "Set case-insensitive matching.", HS_FLAG_CASELESS),
            new RegexFlag("HS_FLAG_DOTALL", "Matching a will not exclude newlines.", HS_FLAG_DOTALL),
            new RegexFlag("HS_FLAG_MULTILINE", "Set multi-line anchoring.", HS_FLAG_MULTILINE),
            new RegexFlag("HS_FLAG_SINGLEMATCH", "Set single-match only mode.", HS_FLAG_SINGLEMATCH),
            new RegexFlag("HS_FLAG_UTF8", "Enable UTF-8 mode for this expression.", HS_FLAG_UTF8),
            new RegexFlag("HS_FLAG_UCP", "Enable Unicode property support for this expression.", HS_FLAG_UCP),
            new RegexFlag("HS_FLAG_COMBINATION", "Logical combination.", HS_FLAG_COMBINATION)
        ];

        OnigFlags =
        [
            new RegexFlag("ONIG_OPTION_IGNORECASE", "Case-insensitive matching.", ONIG_OPTION_IGNORECASE),
            new RegexFlag("ONIG_OPTION_EXTEND", "Extended pattern (verbose).", ONIG_OPTION_EXTEND),
            new RegexFlag("ONIG_OPTION_MULTILINE", "Multi-line mode (^/$ match at line boundaries).", ONIG_OPTION_MULTILINE),
            new RegexFlag("ONIG_OPTION_SINGLELINE", "Single-line mode (. matches newline).", ONIG_OPTION_SINGLELINE),
            new RegexFlag("ONIG_OPTION_FIND_LONGEST", "Find longest match.", ONIG_OPTION_FIND_LONGEST),
            new RegexFlag("ONIG_OPTION_FIND_NOT_EMPTY", "Ignore empty matches.", ONIG_OPTION_FIND_NOT_EMPTY),
            new RegexFlag("ONIG_OPTION_DONT_CAPTURE_GROUP", "Only named groups captured.", ONIG_OPTION_DONT_CAPTURE_GROUP),
            new RegexFlag("ONIG_OPTION_CAPTURE_GROUP", "Named and numbered groups captured.", ONIG_OPTION_CAPTURE_GROUP)
        ];

        OnigSyntaxOptions =
        [
            new OnigSyntaxOption("OnigSyntaxOniguruma", "Oniguruma"),
            new OnigSyntaxOption("OnigSyntaxRuby", "Ruby"),
            new OnigSyntaxOption("OnigSyntaxPerl", "Perl"),
            new OnigSyntaxOption("OnigSyntaxPerl_NG", "Perl (Named Groups)"),
            new OnigSyntaxOption("OnigSyntaxPython", "Python"),
            new OnigSyntaxOption("OnigSyntaxJava", "Java"),
            new OnigSyntaxOption("OnigSyntaxPosixExtended", "POSIX Extended"),
            new OnigSyntaxOption("OnigSyntaxPosixBasic", "POSIX Basic")
        ];

        OnigEncodingOptions =
        [
            new OnigEncodingOption("UTF-8", "UTF-8"),
            new OnigEncodingOption("UTF-16_LE", "UTF-16 LE")
        ];

        DotNetFlags =
        [
            new RegexFlag("RegexOptions.IgnoreCase", "Case-insensitive matching.", (uint)RegexOptions.IgnoreCase),
            new RegexFlag("RegexOptions.Multiline", "Multi-line mode (^/$ match at line boundaries).", (uint)RegexOptions.Multiline),
            new RegexFlag("RegexOptions.Singleline", "Single-line mode (. matches newline).", (uint)RegexOptions.Singleline),
            new RegexFlag("RegexOptions.ExplicitCapture", "Only named groups captured.", (uint)RegexOptions.ExplicitCapture),
            new RegexFlag("RegexOptions.IgnorePatternWhitespace", "Ignore unescaped whitespace and comments.", (uint)RegexOptions.IgnorePatternWhitespace),
            new RegexFlag("RegexOptions.RightToLeft", "Search from right to left.", (uint)RegexOptions.RightToLeft),
            new RegexFlag("RegexOptions.ECMAScript", "ECMAScript-compliant behavior.", (uint)RegexOptions.ECMAScript),
            new RegexFlag("RegexOptions.NonBacktracking", "Use non-backtracking engine (.NET 7+).", (uint)RegexOptions.NonBacktracking)
        ];

        Pcre2Flags =
        [
            new RegexFlag("PCRE2_CASELESS", "Case-insensitive matching.", PCRE2_CASELESS),
            new RegexFlag("PCRE2_MULTILINE", "Multi-line mode (^/$ match at line boundaries).", PCRE2_MULTILINE),
            new RegexFlag("PCRE2_DOTALL", "Single-line mode (. matches newline).", PCRE2_DOTALL),
            new RegexFlag("PCRE2_EXTENDED", "Ignore unescaped whitespace and # comments.", PCRE2_EXTENDED),
            new RegexFlag("PCRE2_UTF", "Enable UTF-8 mode.", PCRE2_UTF) { IsSelected = true },
            new RegexFlag("PCRE2_UCP", "Enable Unicode property support.", PCRE2_UCP),
            new RegexFlag("PCRE2_NO_AUTO_CAPTURE", "Only named groups captured.", PCRE2_NO_AUTO_CAPTURE),
            new RegexFlag("PCRE2_DUPNAMES", "Allow duplicate names for subpatterns.", PCRE2_DUPNAMES),
            new RegexFlag("PCRE2_UNGREEDY", "Invert greedy/lazy quantifiers.", PCRE2_UNGREEDY),
            new RegexFlag("PCRE2_DOLLAR_ENDONLY", "$ matches only at end of subject.", PCRE2_DOLLAR_ENDONLY),
            new RegexFlag("PCRE2_FIRSTLINE", "Match must start in first line.", PCRE2_FIRSTLINE),
            new RegexFlag("PCRE2_LITERAL", "Treat pattern as literal string.", PCRE2_LITERAL),
            new RegexFlag("PCRE2_MATCH_INVALID_UTF", "Allow matching subjects with invalid UTF.", PCRE2_MATCH_INVALID_UTF),
            new RegexFlag("PCRE2_ANCHORED", "Anchor pattern at start of subject.", PCRE2_ANCHORED)
        ];

        SelectedOnigSyntax = OnigSyntaxOptions[0];
        SelectedOnigEncoding = OnigEncodingOptions[0];
    }
}
