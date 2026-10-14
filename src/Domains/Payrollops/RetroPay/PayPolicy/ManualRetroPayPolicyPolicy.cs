namespace Acme.Workforce.Payrollops.RetroPay.PayPolicy;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of manual retro pay policy policy.</summary>
public static class ManualRetroPayPolicyPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 89643.0, in order.</summary>
    public static int ManualRetroPayPolicyBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 89643.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static double ManualRetroPayPolicyWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 2.97;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
