namespace Acme.Workforce.Absence.Policy.Threshold;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of lenient policy threshold policy.</summary>
public static class LenientPolicyThresholdPolicy
{
    public static double LenientPolicyThresholdWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 2.67;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Counts how many amounts fit in a budget of 15878.0, in order.</summary>
    public static int LenientPolicyThresholdBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 15878.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
