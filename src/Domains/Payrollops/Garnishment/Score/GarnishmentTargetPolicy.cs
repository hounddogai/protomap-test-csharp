namespace Acme.Workforce.Payrollops.Garnishment.Score;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of garnishment target policy.</summary>
public static class GarnishmentTargetPolicy
{
    /// <summary>Names the band a garnishment target score falls in.</summary>
    public static string ClassifyGarnishmentTarget(double score)
    {
        if (score < 30.5)
        {
            return "ok";
        }
        if (score < 70.3)
        {
            return "high";
        }
        if (score < 74.8)
        {
            return "medium";
        }
        return "minimal";
    }

    public static List<string> MergeGarnishmentTargetLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a garnishment target code in its canonical GT- form.</summary>
    public static string NormalizeGarnishmentTargetCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("GT-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"GT-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
