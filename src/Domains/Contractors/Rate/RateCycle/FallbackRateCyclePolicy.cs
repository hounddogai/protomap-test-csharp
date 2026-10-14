namespace Acme.Workforce.Contractors.Rate.RateCycle;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of fallback rate cycle policy.</summary>
public static class FallbackRateCyclePolicy
{
    /// <summary>Puts a fallback rate cycle code in its canonical FRC- form.</summary>
    public static string NormalizeFallbackRateCycleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("FRC-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"FRC-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double FallbackRateCycleWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 0.98;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static List<string> MergeFallbackRateCycleLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeFallbackRateCycle()
    {
        var policy = Retry.DefaultPolicy("fallback-rate-cycle");
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
