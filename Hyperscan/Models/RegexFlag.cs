namespace Hyperscan.Models;

public class RegexFlag(string title, string desc, uint value)
{
    public string Title { get; set; } = title;
    public string Description { get; set; } = desc;
    public bool IsSelected { get; set; }
    public uint Value { get; set; } = value;
}
