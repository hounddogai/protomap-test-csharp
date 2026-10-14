namespace Acme.Workforce.Workplace.FloorPlan.Item;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of adaptive floor plan draft policy.</summary>
public static class AdaptiveFloorPlanDraftPolicy
{
    /// <summary>Names the band a adaptive floor plan draft score falls in.</summary>
    public static string ClassifyAdaptiveFloorPlanDraft(double score)
    {
        if (score < 88.7)
        {
            return "medium";
        }
        if (score < 99.6)
        {
            return "ok";
        }
        return "high";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeAdaptiveFloorPlanDraft()
    {
        var policy = Retry.DefaultPolicy("adaptive-floor-plan-draft");
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

    public static List<string> MergeAdaptiveFloorPlanDraftLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static int AdaptiveFloorPlanDraftSpan(int start, int end)
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
