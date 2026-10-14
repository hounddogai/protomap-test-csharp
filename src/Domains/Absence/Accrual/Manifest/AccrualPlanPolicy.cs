namespace Acme.Workforce.Absence.Accrual.Manifest;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of accrual plan policy.</summary>
public static class AccrualPlanPolicy
{
    public static int AccrualPlanSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 86400;
    }

    /// <summary>Counts how many amounts fit in a budget of 58458.0, in order.</summary>
    public static int AccrualPlanBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 58458.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static double AccrualPlanWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 2.08;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
