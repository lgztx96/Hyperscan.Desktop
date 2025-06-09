using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using Hyperscan.Models;
using System.Collections.Generic;
using static Hyperscan.Core.HyperscanApi;

namespace Hyperscan.ViewModels;

public sealed partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    public partial List<MatchContent>? Matches { get; set; }

    [ObservableProperty]
    public partial FlatTreeDataGridSource<MatchContent>? Source { get; set; }

    public List<HsFlag> Flags { get; }

    [ObservableProperty]
    public partial string? FilePath { get; set; }

    public MainViewModel()
    {
        Flags =
        [
            new HsFlag("HS_FLAG_SOM_LEFTMOST", "Enable leftmost start of match reporting.",HS_FLAG_SOM_LEFTMOST) { IsSelected = true },
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
