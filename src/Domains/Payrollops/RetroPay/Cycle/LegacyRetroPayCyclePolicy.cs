namespace Acme.Workforce.Payrollops.RetroPay.Cycle;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of legacy retro pay cycle policy.</summary>
public static class LegacyRetroPayCyclePolicy
{
    /// <summary>Names the band a legacy retro pay cycle score falls in.</summary>
    public static string ClassifyLegacyRetroPayCycle(double score)
    {
        if (score < 21.0)
        {
            return "critical";
        }
        if (score < 92.6)
        {
            return "high";
        }
        if (score < 99.9)
        {
            return "minimal";
        }
        return "low";
    }

    public static List<string> MergeLegacyRetroPayCycleLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static string LegacyRetroPayCycleBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    public static double LegacyRetroPayCycleWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 0.88;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
