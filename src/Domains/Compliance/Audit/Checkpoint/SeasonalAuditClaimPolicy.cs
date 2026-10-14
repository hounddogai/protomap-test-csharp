namespace Acme.Workforce.Compliance.Audit.Checkpoint;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of seasonal audit claim policy.</summary>
public static class SeasonalAuditClaimPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 3303.0, in order.</summary>
    public static int SeasonalAuditClaimBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 3303.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string SeasonalAuditClaimBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }
}
