namespace Hyperscan.Models;

public class OnigEncodingOption(string name, string displayName)
{
    public string Name { get; set; } = name;
    public string DisplayName { get; set; } = displayName;
}
