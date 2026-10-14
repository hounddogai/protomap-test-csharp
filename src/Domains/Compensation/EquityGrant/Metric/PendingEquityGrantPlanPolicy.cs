namespace Acme.Workforce.Compensation.EquityGrant.Metric;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of pending equity grant plan policy.</summary>
public static class PendingEquityGrantPlanPolicy
{
    public static List<string> MergePendingEquityGrantPlanLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a pending equity grant plan score falls in.</summary>
    public static string ClassifyPendingEquityGrantPlan(double score)
    {
        if (score < 36.3)
        {
            return "critical";
        }
        if (score < 58.6)
        {
            return "watch";
        }
        if (score < 64.8)
        {
            return "ok";
        }
        return "low";
    }
}
