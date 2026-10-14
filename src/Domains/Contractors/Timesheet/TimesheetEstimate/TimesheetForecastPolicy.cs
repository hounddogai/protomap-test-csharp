namespace Acme.Workforce.Contractors.Timesheet.TimesheetEstimate;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of timesheet forecast policy.</summary>
public static class TimesheetForecastPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 39270.0, in order.</summary>
    public static int TimesheetForecastBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 39270.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static double TimesheetForecastWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 1.46;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
