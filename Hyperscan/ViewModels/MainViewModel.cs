using CommunityToolkit.Mvvm.ComponentModel;
using Hyperscan.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using static Hyperscan.Core.HyperscanApi;

namespace Hyperscan.ViewModels;

public sealed partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<MatchContent>? matches;

    public List<HsFlag> Flags { get; }

    //public string? Pattern { get; set; }

    //public string? Text { get; set; }

    //public string? HexText { get; set; }

    [ObservableProperty]
    private string? filePath;

    public MainViewModel()
    {
        Flags =
        [
            new HsFlag("HS_FLAG_CASELESS", "Set case-insensitive matching.",HS_FLAG_CASELESS),
            new HsFlag("HS_FLAG_DOTALL", "Matching a will not exclude newlines.",HS_FLAG_DOTALL),
            new HsFlag("HS_FLAG_MULTILINE", "Set multi-line anchoring.",HS_FLAG_MULTILINE),
            new HsFlag("HS_FLAG_SINGLEMATCH", "Set single-match only mode.",HS_FLAG_SINGLEMATCH),
            new HsFlag("HS_FLAG_UTF8", "Enable UTF-8 mode for this expression.",HS_FLAG_UTF8),
            new HsFlag("HS_FLAG_UCP", "Enable Unicode property support for this expression.",HS_FLAG_UCP),
            new HsFlag("HS_FLAG_COMBINATION", "Logical combination.",HS_FLAG_COMBINATION)
        ];
    }
}
