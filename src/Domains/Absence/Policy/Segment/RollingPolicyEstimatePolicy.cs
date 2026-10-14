namespace Acme.Workforce.Absence.Policy.Segment;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of rolling policy estimate policy.</summary>
public static class RollingPolicyEstimatePolicy
{
    public static double RollingPolicyEstimateWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 2.56;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static List<string> MergeRollingPolicyEstimateLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeRollingPolicyEstimate()
    {
        var policy = Retry.DefaultPolicy("rolling-policy-estimate");
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

    /// <summary>Puts a rolling policy estimate code in its canonical RPE- form.</summary>
    public static string NormalizeRollingPolicyEstimateCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RPE-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RPE-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
