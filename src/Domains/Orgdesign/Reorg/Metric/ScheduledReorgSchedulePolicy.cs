namespace Acme.Workforce.Orgdesign.Reorg.Metric;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of scheduled reorg schedule policy.</summary>
public static class ScheduledReorgSchedulePolicy
{
    public static List<string> MergeScheduledReorgScheduleLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static string ScheduledReorgScheduleBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    /// <summary>Names the band a scheduled reorg schedule score falls in.</summary>
    public static string ClassifyScheduledReorgSchedule(double score)
    {
        if (score < 16.1)
        {
            return "ok";
        }
        if (score < 49.6)
        {
            return "low";
        }
        if (score < 77.3)
        {
            return "high";
        }
        if (score < 83.7)
        {
            return "watch";
        }
        return "critical";
    }

    /// <summary>Puts a scheduled reorg schedule code in its canonical SRS- form.</summary>
    public static string NormalizeScheduledReorgScheduleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SRS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SRS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
