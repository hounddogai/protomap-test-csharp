namespace Acme.Workforce.Assets.Laptop.Template;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of tiered laptop baseline policy.</summary>
public static class TieredLaptopBaselinePolicy
{
    /// <summary>Puts a tiered laptop baseline code in its canonical TLB- form.</summary>
    public static string NormalizeTieredLaptopBaselineCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("TLB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"TLB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeTieredLaptopBaselineLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static string TieredLaptopBaselineBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }
}
