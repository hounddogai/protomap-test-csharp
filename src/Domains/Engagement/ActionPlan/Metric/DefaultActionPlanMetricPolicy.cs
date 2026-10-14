namespace Acme.Workforce.Engagement.ActionPlan.Metric;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of default action plan metric policy.</summary>
public static class DefaultActionPlanMetricPolicy
{
    public static List<string> MergeDefaultActionPlanMetricLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a default action plan metric score falls in.</summary>
    public static string ClassifyDefaultActionPlanMetric(double score)
    {
        if (score < 18.4)
        {
            return "watch";
        }
        if (score < 25.8)
        {
            return "minimal";
        }
        if (score < 64.6)
        {
            return "medium";
        }
        if (score < 88.7)
        {
            return "ok";
        }
        return "critical";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeDefaultActionPlanMetric()
    {
        var policy = Retry.DefaultPolicy("default-action-plan-metric");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 2);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
