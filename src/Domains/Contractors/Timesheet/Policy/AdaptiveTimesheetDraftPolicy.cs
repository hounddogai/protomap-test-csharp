namespace Acme.Workforce.Contractors.Timesheet.Policy;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of adaptive timesheet draft policy.</summary>
public static class AdaptiveTimesheetDraftPolicy
{
    public static int AdaptiveTimesheetDraftSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 60;
    }

    /// <summary>Counts how many amounts fit in a budget of 65293.0, in order.</summary>
    public static int AdaptiveTimesheetDraftBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 65293.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static double AdaptiveTimesheetDraftWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 2.2;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
