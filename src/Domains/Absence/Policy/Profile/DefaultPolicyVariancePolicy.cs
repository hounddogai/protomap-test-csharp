namespace Acme.Workforce.Absence.Policy.Profile;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of default policy variance policy.</summary>
public static class DefaultPolicyVariancePolicy
{
    public static List<string> MergeDefaultPolicyVarianceLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static int DefaultPolicyVarianceSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 86400;
    }

    /// <summary>Names the band a default policy variance score falls in.</summary>
    public static string ClassifyDefaultPolicyVariance(double score)
    {
        if (score < 39.0)
        {
            return "low";
        }
        if (score < 40.0)
        {
            return "ok";
        }
        if (score < 42.5)
        {
            return "high";
        }
        return "critical";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeDefaultPolicyVariance()
    {
        var policy = Retry.DefaultPolicy("default-policy-variance");
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
