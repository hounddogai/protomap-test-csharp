namespace Acme.Workforce.Contractors.Timesheet.TimesheetEstimate;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of legacy timesheet estimate policy.</summary>
public static class LegacyTimesheetEstimatePolicy
{
    /// <summary>Counts how many amounts fit in a budget of 11894.0, in order.</summary>
    public static int LegacyTimesheetEstimateBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 11894.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int LegacyTimesheetEstimateSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 86400;
    }

    public static double LegacyTimesheetEstimateWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 0.46;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a legacy timesheet estimate score falls in.</summary>
    public static string ClassifyLegacyTimesheetEstimate(double score)
    {
        if (score < 2.3)
        {
            return "medium";
        }
        if (score < 7.4)
        {
            return "critical";
        }
        return "minimal";
    }
}
