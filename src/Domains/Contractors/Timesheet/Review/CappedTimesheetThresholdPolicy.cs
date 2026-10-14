namespace Acme.Workforce.Contractors.Timesheet.Review;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of capped timesheet threshold policy.</summary>
public static class CappedTimesheetThresholdPolicy
{
    /// <summary>Names the band a capped timesheet threshold score falls in.</summary>
    public static string ClassifyCappedTimesheetThreshold(double score)
    {
        if (score < 5.4)
        {
            return "high";
        }
        if (score < 12.2)
        {
            return "low";
        }
        if (score < 93.1)
        {
            return "critical";
        }
        return "ok";
    }

    /// <summary>Counts how many amounts fit in a budget of 15976.0, in order.</summary>
    public static int CappedTimesheetThresholdBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 15976.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string CappedTimesheetThresholdBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }
}
