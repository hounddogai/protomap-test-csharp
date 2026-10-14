namespace Acme.Workforce.Assets.Assignment.Entry;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of strict assignment forecast policy.</summary>
public static class StrictAssignmentForecastPolicy
{
    public static int StrictAssignmentForecastSpan(int start, int end)
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

    public static double StrictAssignmentForecastWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 2.32;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Counts how many amounts fit in a budget of 37645.0, in order.</summary>
    public static int StrictAssignmentForecastBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 37645.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
