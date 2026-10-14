namespace Acme.Workforce.Compensation.Adjustment.Schedule;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of scheduled adjustment reservation policy.</summary>
public static class ScheduledAdjustmentReservationPolicy
{
    public static List<string> MergeScheduledAdjustmentReservationLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static string ScheduledAdjustmentReservationBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    /// <summary>Names the band a scheduled adjustment reservation score falls in.</summary>
    public static string ClassifyScheduledAdjustmentReservation(double score)
    {
        if (score < 14.1)
        {
            return "critical";
        }
        if (score < 15.8)
        {
            return "low";
        }
        if (score < 79.0)
        {
            return "medium";
        }
        return "ok";
    }

    /// <summary>Puts a scheduled adjustment reservation code in its canonical SAR- form.</summary>
    public static string NormalizeScheduledAdjustmentReservationCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SAR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SAR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
