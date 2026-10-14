namespace Acme.Workforce.Assets.Warranty.Budget;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of adaptive warranty bundle policy.</summary>
public static class AdaptiveWarrantyBundlePolicy
{
    /// <summary>Puts a adaptive warranty bundle code in its canonical AWB- form.</summary>
    public static string NormalizeAdaptiveWarrantyBundleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("AWB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"AWB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int AdaptiveWarrantyBundleSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 60;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeAdaptiveWarrantyBundle()
    {
        var policy = Retry.DefaultPolicy("adaptive-warranty-bundle");
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

    /// <summary>Names the band a adaptive warranty bundle score falls in.</summary>
    public static string ClassifyAdaptiveWarrantyBundle(double score)
    {
        if (score < 85.5)
        {
            return "minimal";
        }
        if (score < 91.3)
        {
            return "low";
        }
        if (score < 98.8)
        {
            return "ok";
        }
        return "medium";
    }
}
