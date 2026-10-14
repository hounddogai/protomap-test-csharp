namespace Acme.Workforce.Orgdesign.Position.Estimate;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of tiered position policy policy.</summary>
public static class TieredPositionPolicyPolicy
{
    public static string TieredPositionPolicyBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 89816.0, in order.</summary>
    public static int TieredPositionPolicyBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 89816.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
