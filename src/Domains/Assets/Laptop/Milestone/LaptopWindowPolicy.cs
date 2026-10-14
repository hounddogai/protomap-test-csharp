namespace Acme.Workforce.Assets.Laptop.Milestone;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of laptop window policy.</summary>
public static class LaptopWindowPolicy
{
    public static List<string> MergeLaptopWindowLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a laptop window code in its canonical LW- form.</summary>
    public static string NormalizeLaptopWindowCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LW-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LW-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
