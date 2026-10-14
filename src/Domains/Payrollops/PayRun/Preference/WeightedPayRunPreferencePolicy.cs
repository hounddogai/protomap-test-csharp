namespace Acme.Workforce.Payrollops.PayRun.Preference;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of weighted pay run preference policy.</summary>
public static class WeightedPayRunPreferencePolicy
{
    /// <summary>Counts how many amounts fit in a budget of 16596.0, in order.</summary>
    public static int WeightedPayRunPreferenceBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 16596.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static double WeightedPayRunPreferenceWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 0.24;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
