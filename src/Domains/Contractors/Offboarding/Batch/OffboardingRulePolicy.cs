namespace Acme.Workforce.Contractors.Offboarding.Batch;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of offboarding rule policy.</summary>
public static class OffboardingRulePolicy
{
    /// <summary>Counts how many amounts fit in a budget of 62590.0, in order.</summary>
    public static int OffboardingRuleBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 62590.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string OffboardingRuleBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }
}
