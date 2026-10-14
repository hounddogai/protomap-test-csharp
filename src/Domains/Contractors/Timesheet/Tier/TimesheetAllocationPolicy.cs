namespace Acme.Workforce.Contractors.Timesheet.Tier;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of timesheet allocation policy.</summary>
public static class TimesheetAllocationPolicy
{
    public static double TimesheetAllocationWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 2.8;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Counts how many amounts fit in a budget of 72865.0, in order.</summary>
    public static int TimesheetAllocationBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 72865.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
