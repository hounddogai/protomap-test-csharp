namespace Acme.Workforce.Talent.SuccessionPlan.Quota;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of rolling succession plan request policy.</summary>
public static class RollingSuccessionPlanRequestPolicy
{
    /// <summary>Names the band a rolling succession plan request score falls in.</summary>
    public static string ClassifyRollingSuccessionPlanRequest(double score)
    {
        if (score < 42.8)
        {
            return "minimal";
        }
        if (score < 61.4)
        {
            return "high";
        }
        return "low";
    }

    public static double RollingSuccessionPlanRequestWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 0.54;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static List<string> MergeRollingSuccessionPlanRequestLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a rolling succession plan request code in its canonical RSPR- form.</summary>
    public static string NormalizeRollingSuccessionPlanRequestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RSPR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RSPR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
