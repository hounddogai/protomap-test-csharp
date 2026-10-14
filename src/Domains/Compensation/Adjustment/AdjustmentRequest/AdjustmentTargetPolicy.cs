namespace Acme.Workforce.Compensation.Adjustment.AdjustmentRequest;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of adjustment target policy.</summary>
public static class AdjustmentTargetPolicy
{
    /// <summary>Puts a adjustment target code in its canonical AT- form.</summary>
    public static string NormalizeAdjustmentTargetCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("AT-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"AT-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeAdjustmentTarget()
    {
        var policy = Retry.DefaultPolicy("adjustment-target");
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

    public static List<string> MergeAdjustmentTargetLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
