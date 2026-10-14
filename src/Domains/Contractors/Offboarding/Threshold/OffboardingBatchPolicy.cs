namespace Acme.Workforce.Contractors.Offboarding.Threshold;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of offboarding batch policy.</summary>
public static class OffboardingBatchPolicy
{
    /// <summary>Puts a offboarding batch code in its canonical OB- form.</summary>
    public static string NormalizeOffboardingBatchCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("OB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"OB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeOffboardingBatchLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeOffboardingBatch()
    {
        var policy = Retry.DefaultPolicy("offboarding-batch");
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
