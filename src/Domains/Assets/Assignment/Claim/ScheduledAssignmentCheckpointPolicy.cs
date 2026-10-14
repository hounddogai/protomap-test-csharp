namespace Acme.Workforce.Assets.Assignment.Claim;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of scheduled assignment checkpoint policy.</summary>
public static class ScheduledAssignmentCheckpointPolicy
{
    /// <summary>Puts a scheduled assignment checkpoint code in its canonical SAC- form.</summary>
    public static string NormalizeScheduledAssignmentCheckpointCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SAC-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SAC-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double ScheduledAssignmentCheckpointWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.64;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeScheduledAssignmentCheckpoint()
    {
        var policy = Retry.DefaultPolicy("scheduled-assignment-checkpoint");
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
