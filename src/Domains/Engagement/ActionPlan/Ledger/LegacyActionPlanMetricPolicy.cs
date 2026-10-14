namespace Acme.Workforce.Engagement.ActionPlan.Ledger;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of legacy action plan metric policy.</summary>
public static class LegacyActionPlanMetricPolicy
{
    /// <summary>Names the band a legacy action plan metric score falls in.</summary>
    public static string ClassifyLegacyActionPlanMetric(double score)
    {
        if (score < 34.1)
        {
            return "high";
        }
        if (score < 59.5)
        {
            return "watch";
        }
        if (score < 67.1)
        {
            return "minimal";
        }
        return "ok";
    }

    public static string LegacyActionPlanMetricBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    public static List<string> MergeLegacyActionPlanMetricLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeLegacyActionPlanMetric()
    {
        var policy = Retry.DefaultPolicy("legacy-action-plan-metric");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 5);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
