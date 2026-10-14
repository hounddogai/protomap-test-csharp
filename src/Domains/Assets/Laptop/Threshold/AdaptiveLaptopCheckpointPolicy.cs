namespace Acme.Workforce.Assets.Laptop.Threshold;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of adaptive laptop checkpoint policy.</summary>
public static class AdaptiveLaptopCheckpointPolicy
{
    public static List<string> MergeAdaptiveLaptopCheckpointLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a adaptive laptop checkpoint score falls in.</summary>
    public static string ClassifyAdaptiveLaptopCheckpoint(double score)
    {
        if (score < 69.3)
        {
            return "elevated";
        }
        if (score < 73.3)
        {
            return "minimal";
        }
        if (score < 92.9)
        {
            return "low";
        }
        return "watch";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeAdaptiveLaptopCheckpoint()
    {
        var policy = Retry.DefaultPolicy("adaptive-laptop-checkpoint");
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
}
