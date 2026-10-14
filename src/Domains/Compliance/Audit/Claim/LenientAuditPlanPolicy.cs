namespace Acme.Workforce.Compliance.Audit.Claim;

/// <summary>Functions of lenient audit plan policy.</summary>
public static class LenientAuditPlanPolicy
{
    /// <summary>Names the band a lenient audit plan score falls in.</summary>
    public static string ClassifyLenientAuditPlan(double score)
    {
        if (score < 72.1)
        {
            return "minimal";
        }
        if (score < 80.8)
        {
            return "watch";
        }
        return "high";
    }

    /// <summary>Counts how many amounts fit in a budget of 57236.0, in order.</summary>
    public static int LenientAuditPlanBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 57236.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int LenientAuditPlanSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 60;
    }
}
