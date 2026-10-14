namespace Acme.Workforce.Compliance.Audit.RecordItem;

/// <summary>Functions of strict audit manifest policy.</summary>
public static class StrictAuditManifestPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 66927.0, in order.</summary>
    public static int StrictAuditManifestBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 66927.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int StrictAuditManifestSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 86400;
    }
}
