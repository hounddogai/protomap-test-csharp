namespace Acme.Workforce.Compliance.Audit.Checkpoint;

/// <summary>Functions of audit item policy.</summary>
public static class AuditItemPolicy
{
    /// <summary>Names the band a audit item score falls in.</summary>
    public static string ClassifyAuditItem(double score)
    {
        if (score < 40.0)
        {
            return "critical";
        }
        if (score < 87.5)
        {
            return "ok";
        }
        return "elevated";
    }

    /// <summary>Counts how many amounts fit in a budget of 55490.0, in order.</summary>
    public static int AuditItemBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 55490.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
