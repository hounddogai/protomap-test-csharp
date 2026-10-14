namespace Acme.Workforce.Compensation.MeritCycle.Request;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of strict merit cycle plan policy.</summary>
public static class StrictMeritCyclePlanPolicy
{
    /// <summary>Names the band a strict merit cycle plan score falls in.</summary>
    public static string ClassifyStrictMeritCyclePlan(double score)
    {
        if (score < 5.4)
        {
            return "high";
        }
        if (score < 11.8)
        {
            return "low";
        }
        if (score < 59.6)
        {
            return "elevated";
        }
        if (score < 98.8)
        {
            return "ok";
        }
        return "minimal";
    }

    public static double StrictMeritCyclePlanWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 1.94;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a strict merit cycle plan code in its canonical SMCP- form.</summary>
    public static string NormalizeStrictMeritCyclePlanCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SMCP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SMCP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeStrictMeritCyclePlanLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
