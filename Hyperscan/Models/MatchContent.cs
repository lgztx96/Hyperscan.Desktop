using CommunityToolkit.Mvvm.ComponentModel;

namespace Hyperscan.Models;

public sealed partial class MatchContent(uint offset, string content, string hex) : ObservableObject
{
    [ObservableProperty]
    public partial string Content { get; set; } = content;

    [ObservableProperty]
    public partial uint Offset { get; set; } = offset;

    [ObservableProperty]
    public partial string Hex { get; set; } = hex;
}
