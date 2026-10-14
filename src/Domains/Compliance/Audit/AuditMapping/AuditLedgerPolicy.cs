namespace Acme.Workforce.Compliance.Audit.AuditMapping;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of audit ledger policy.</summary>
public static class AuditLedgerPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 75625.0, in order.</summary>
    public static int AuditLedgerBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 75625.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a audit ledger score falls in.</summary>
    public static string ClassifyAuditLedger(double score)
    {
        if (score < 6.5)
        {
            return "critical";
        }
        if (score < 14.3)
        {
            return "ok";
        }
        if (score < 45.8)
        {
            return "elevated";
        }
        if (score < 80.5)
        {
            return "watch";
        }
        return "high";
    }

    public static string AuditLedgerBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }
}
