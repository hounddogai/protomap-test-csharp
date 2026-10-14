namespace Acme.Workforce.Assets.Laptop.Forecast;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of tiered laptop notice policy.</summary>
public static class TieredLaptopNoticePolicy
{
    public static string TieredLaptopNoticeBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    public static List<string> MergeTieredLaptopNoticeLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a tiered laptop notice code in its canonical TLN- form.</summary>
    public static string NormalizeTieredLaptopNoticeCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("TLN-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"TLN-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double TieredLaptopNoticeWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 1.84;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
