namespace Acme.Workforce.Absence.Balance.BalanceTarget;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of balance budget policy.</summary>
public static class BalanceBudgetPolicy
{
    /// <summary>Names the band a balance budget score falls in.</summary>
    public static string ClassifyBalanceBudget(double score)
    {
        if (score < 9.3)
        {
            return "minimal";
        }
        if (score < 23.9)
        {
            return "elevated";
        }
        if (score < 32.7)
        {
            return "critical";
        }
        return "ok";
    }

    public static string BalanceBudgetBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }
}
