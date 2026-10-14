namespace Acme.Workforce.Payrollops.PayRun.Preference;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of scheduled pay run request policy.</summary>
public static class ScheduledPayRunRequestPolicy
{
    public static string ScheduledPayRunRequestBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    public static List<string> MergeScheduledPayRunRequestLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a scheduled pay run request score falls in.</summary>
    public static string ClassifyScheduledPayRunRequest(double score)
    {
        if (score < 65.8)
        {
            return "low";
        }
        if (score < 83.1)
        {
            return "minimal";
        }
        return "ok";
    }
}
