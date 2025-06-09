using CommunityToolkit.Mvvm.ComponentModel;

namespace Hyperscan.Models;

public sealed partial class MatchContent(uint offset, string content, string hex, int byteLen) : ObservableObject
{
    public string Content { get; set; } = content;

    public uint Offset { get; set; } = offset;

    public string Hex { get; set; } = hex;

    public int ByteLength { get; set; } = byteLen;
}
