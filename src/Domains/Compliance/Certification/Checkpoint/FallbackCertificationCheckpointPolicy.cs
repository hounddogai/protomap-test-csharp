namespace Acme.Workforce.Compliance.Certification.Checkpoint;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of fallback certification checkpoint policy.</summary>
public static class FallbackCertificationCheckpointPolicy
{
    /// <summary>Names the band a fallback certification checkpoint score falls in.</summary>
    public static string ClassifyFallbackCertificationCheckpoint(double score)
    {
        if (score < 47.5)
        {
            return "high";
        }
        if (score < 68.8)
        {
            return "elevated";
        }
        return "critical";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeFallbackCertificationCheckpoint()
    {
        var policy = Retry.DefaultPolicy("fallback-certification-checkpoint");
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

    public static double FallbackCertificationCheckpointWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 2.52;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static List<string> MergeFallbackCertificationCheckpointLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
