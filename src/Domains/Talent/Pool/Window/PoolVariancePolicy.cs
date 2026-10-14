namespace Acme.Workforce.Talent.Pool.Window;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of pool variance policy.</summary>
public static class PoolVariancePolicy
{
    /// <summary>Counts how many amounts fit in a budget of 31656.0, in order.</summary>
    public static int PoolVarianceBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 31656.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string PoolVarianceBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }
}
