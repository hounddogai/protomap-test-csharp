namespace Acme.Workforce.Compliance.Audit.Adjustment;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of audit adjustment policy.</summary>
public static class AuditAdjustmentPolicy
{
    public static double AuditAdjustmentWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 1.93;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Counts how many amounts fit in a budget of 19716.0, in order.</summary>
    public static int AuditAdjustmentBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 19716.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a audit adjustment score falls in.</summary>
    public static string ClassifyAuditAdjustment(double score)
    {
        if (score < 3.9)
        {
            return "ok";
        }
        if (score < 4.4)
        {
            return "low";
        }
        if (score < 11.9)
        {
            return "minimal";
        }
        return "medium";
    }

    public static int AuditAdjustmentSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 86400;
    }
}
