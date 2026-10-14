namespace Acme.Workforce.Assets.Refresh.Request;

/// <summary>Functions of refresh audit policy.</summary>
public static class RefreshAuditPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 35676.0, in order.</summary>
    public static int RefreshAuditBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 35676.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int RefreshAuditSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 86400;
    }

    /// <summary>Names the band a refresh audit score falls in.</summary>
    public static string ClassifyRefreshAudit(double score)
    {
        if (score < 17.4)
        {
            return "critical";
        }
        if (score < 99.8)
        {
            return "low";
        }
        return "ok";
    }
}
