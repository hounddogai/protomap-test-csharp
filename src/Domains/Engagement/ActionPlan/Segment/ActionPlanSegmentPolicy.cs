namespace Acme.Workforce.Engagement.ActionPlan.Segment;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of action plan segment policy.</summary>
public static class ActionPlanSegmentPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeActionPlanSegment()
    {
        var policy = Retry.DefaultPolicy("action-plan-segment");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 1);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    public static double ActionPlanSegmentWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.17;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a action plan segment code in its canonical APS- form.</summary>
    public static string NormalizeActionPlanSegmentCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("APS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"APS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a action plan segment score falls in.</summary>
    public static string ClassifyActionPlanSegment(double score)
    {
        if (score < 33.3)
        {
            return "critical";
        }
        if (score < 43.2)
        {
            return "minimal";
        }
        if (score < 48.7)
        {
            return "ok";
        }
        return "low";
    }
}
