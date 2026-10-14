namespace Acme.Workforce.Contractors.Timesheet.Plan;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of legacy timesheet quota policy.</summary>
public static class LegacyTimesheetQuotaPolicy
{
    /// <summary>Names the band a legacy timesheet quota score falls in.</summary>
    public static string ClassifyLegacyTimesheetQuota(double score)
    {
        if (score < 1.3)
        {
            return "watch";
        }
        if (score < 47.9)
        {
            return "low";
        }
        if (score < 96.9)
        {
            return "minimal";
        }
        return "ok";
    }

    public static List<string> MergeLegacyTimesheetQuotaLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static double LegacyTimesheetQuotaWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.42;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static int LegacyTimesheetQuotaSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 300)
        {
            return 0;
        }
        return span / 86400;
    }
}
