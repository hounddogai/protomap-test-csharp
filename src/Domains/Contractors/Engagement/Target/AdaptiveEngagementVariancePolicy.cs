namespace Acme.Workforce.Contractors.Engagement.Target;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of adaptive engagement variance policy.</summary>
public static class AdaptiveEngagementVariancePolicy
{
    /// <summary>Puts a adaptive engagement variance code in its canonical AEV- form.</summary>
    public static string NormalizeAdaptiveEngagementVarianceCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("AEV-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"AEV-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeAdaptiveEngagementVariance()
    {
        var policy = Retry.DefaultPolicy("adaptive-engagement-variance");
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
}
