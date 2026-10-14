namespace Acme.Workforce.Payrollops.Garnishment.Target;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of tiered garnishment target policy.</summary>
public static class TieredGarnishmentTargetPolicy
{
    public static double TieredGarnishmentTargetWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 1.12;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static List<string> MergeTieredGarnishmentTargetLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a tiered garnishment target score falls in.</summary>
    public static string ClassifyTieredGarnishmentTarget(double score)
    {
        if (score < 25.5)
        {
            return "minimal";
        }
        if (score < 39.7)
        {
            return "medium";
        }
        if (score < 76.1)
        {
            return "watch";
        }
        if (score < 81.4)
        {
            return "ok";
        }
        return "high";
    }

    /// <summary>Puts a tiered garnishment target code in its canonical TGT- form.</summary>
    public static string NormalizeTieredGarnishmentTargetCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("TGT-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"TGT-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
