using CommunityToolkit.Mvvm.ComponentModel;

namespace Hyperscan.Models;

public partial class MatchContent(uint offset, string content, string hex) : ObservableObject
{
    [ObservableProperty]
    private string content = content;

    [ObservableProperty]
    private uint offset = offset;

    [ObservableProperty]
    private string hex = hex;
}
