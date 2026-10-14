namespace Acme.Workforce.Talent.CareerPath.PathSummary;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of regional career path summary policy.</summary>
public static class RegionalCareerPathSummaryPolicy
{
    public static string RegionalCareerPathSummaryBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    /// <summary>Puts a regional career path summary code in its canonical RCPS- form.</summary>
    public static string NormalizeRegionalCareerPathSummaryCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RCPS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RCPS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeRegionalCareerPathSummaryLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static double RegionalCareerPathSummaryWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 0.28;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
