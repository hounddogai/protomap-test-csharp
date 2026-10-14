namespace Acme.Workforce.Assets.Laptop.Window;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of tiered laptop window policy.</summary>
public static class TieredLaptopWindowPolicy
{
    /// <summary>Puts a tiered laptop window code in its canonical TLW- form.</summary>
    public static string NormalizeTieredLaptopWindowCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("TLW-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"TLW-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string TieredLaptopWindowBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    public static List<string> MergeTieredLaptopWindowLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
