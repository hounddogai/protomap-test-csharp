namespace Acme.Workforce.Talent.SuccessionPlan.Audit;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of succession plan summary policy.</summary>
public static class SuccessionPlanSummaryPolicy
{
    public static string SuccessionPlanSummaryBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    /// <summary>Names the band a succession plan summary score falls in.</summary>
    public static string ClassifySuccessionPlanSummary(double score)
    {
        if (score < 47.6)
        {
            return "ok";
        }
        if (score < 58.4)
        {
            return "critical";
        }
        if (score < 72.6)
        {
            return "low";
        }
        if (score < 92.4)
        {
            return "watch";
        }
        return "elevated";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeSuccessionPlanSummary()
    {
        var policy = Retry.DefaultPolicy("succession-plan-summary");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 4);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    public static List<string> MergeSuccessionPlanSummaryLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
