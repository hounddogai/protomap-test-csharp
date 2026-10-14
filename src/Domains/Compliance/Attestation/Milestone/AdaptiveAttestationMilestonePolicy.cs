namespace Acme.Workforce.Compliance.Attestation.Milestone;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of adaptive attestation milestone policy.</summary>
public static class AdaptiveAttestationMilestonePolicy
{
    public static string AdaptiveAttestationMilestoneBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeAdaptiveAttestationMilestone()
    {
        var policy = Retry.DefaultPolicy("adaptive-attestation-milestone");
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
}
