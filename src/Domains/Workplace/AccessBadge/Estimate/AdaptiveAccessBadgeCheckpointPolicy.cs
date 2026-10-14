namespace Acme.Workforce.Workplace.AccessBadge.Estimate;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of adaptive access badge checkpoint policy.</summary>
public static class AdaptiveAccessBadgeCheckpointPolicy
{
    public static double AdaptiveAccessBadgeCheckpointWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 0.61;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeAdaptiveAccessBadgeCheckpoint()
    {
        var policy = Retry.DefaultPolicy("adaptive-access-badge-checkpoint");
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
