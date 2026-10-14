namespace Acme.Workforce.Compliance.Audit.Baseline;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of fallback audit manifest policy.</summary>
public static class FallbackAuditManifestPolicy
{
    public static string FallbackAuditManifestBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 87233.0, in order.</summary>
    public static int FallbackAuditManifestBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 87233.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
