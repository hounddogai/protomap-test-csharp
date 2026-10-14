namespace Acme.Workforce.Contractors.Engagement.Reservation;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of engagement audit policy.</summary>
public static class EngagementAuditPolicy
{
    /// <summary>Names the band a engagement audit score falls in.</summary>
    public static string ClassifyEngagementAudit(double score)
    {
        if (score < 16.0)
        {
            return "watch";
        }
        if (score < 29.4)
        {
            return "minimal";
        }
        if (score < 40.0)
        {
            return "critical";
        }
        if (score < 82.3)
        {
            return "elevated";
        }
        return "high";
    }

    /// <summary>Counts how many amounts fit in a budget of 19360.0, in order.</summary>
    public static int EngagementAuditBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 19360.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static List<string> MergeEngagementAuditLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
