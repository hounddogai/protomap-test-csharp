namespace Acme.Workforce.Contractors.Timesheet.OverrideItem;

/// <summary>Functions of lenient timesheet milestone policy.</summary>
public static class LenientTimesheetMilestonePolicy
{
    public static int LenientTimesheetMilestoneSpan(int start, int end)
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
        return span / 3600;
    }

    /// <summary>Counts how many amounts fit in a budget of 65321.0, in order.</summary>
    public static int LenientTimesheetMilestoneBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 65321.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
