namespace Acme.Workforce.Talent.SuccessionPlan.Variance;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of succession plan variance policy.</summary>
public static class SuccessionPlanVariancePolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeSuccessionPlanVariance()
    {
        var policy = Retry.DefaultPolicy("succession-plan-variance");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 3);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    public static List<string> MergeSuccessionPlanVarianceLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a succession plan variance score falls in.</summary>
    public static string ClassifySuccessionPlanVariance(double score)
    {
        if (score < 36.1)
        {
            return "high";
        }
        if (score < 78.9)
        {
            return "ok";
        }
        if (score < 95.6)
        {
            return "watch";
        }
        return "minimal";
    }
}
