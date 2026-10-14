namespace Acme.Workforce.Engagement.ActionPlan.Bundle;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of lenient action plan bundle policy.</summary>
public static class LenientActionPlanBundlePolicy
{
    /// <summary>Names the band a lenient action plan bundle score falls in.</summary>
    public static string ClassifyLenientActionPlanBundle(double score)
    {
        if (score < 26.0)
        {
            return "medium";
        }
        if (score < 55.9)
        {
            return "elevated";
        }
        return "ok";
    }

    public static List<string> MergeLenientActionPlanBundleLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeLenientActionPlanBundle()
    {
        var policy = Retry.DefaultPolicy("lenient-action-plan-bundle");
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
