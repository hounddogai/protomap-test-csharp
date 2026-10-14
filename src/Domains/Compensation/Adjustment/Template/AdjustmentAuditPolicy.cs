namespace Acme.Workforce.Compensation.Adjustment.Template;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of adjustment audit policy.</summary>
public static class AdjustmentAuditPolicy
{
    public static string AdjustmentAuditBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 72627.0, in order.</summary>
    public static int AdjustmentAuditBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 72627.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a adjustment audit score falls in.</summary>
    public static string ClassifyAdjustmentAudit(double score)
    {
        if (score < 2.7)
        {
            return "critical";
        }
        if (score < 26.4)
        {
            return "high";
        }
        if (score < 51.9)
        {
            return "medium";
        }
        if (score < 72.5)
        {
            return "watch";
        }
        return "low";
    }
}
