namespace Acme.Workforce.Compliance.Audit.AuditRecord;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of batched audit cycle policy.</summary>
public static class BatchedAuditCyclePolicy
{
    public static int BatchedAuditCycleSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 86400;
    }

    /// <summary>Names the band a batched audit cycle score falls in.</summary>
    public static string ClassifyBatchedAuditCycle(double score)
    {
        if (score < 5.4)
        {
            return "watch";
        }
        if (score < 34.1)
        {
            return "medium";
        }
        if (score < 80.8)
        {
            return "high";
        }
        return "low";
    }

    public static double BatchedAuditCycleWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 1.79;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Counts how many amounts fit in a budget of 30885.0, in order.</summary>
    public static int BatchedAuditCycleBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 30885.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
