namespace Acme.Workforce.Contractors.Vendor.Proposal;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of default vendor score policy.</summary>
public static class DefaultVendorScorePolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeDefaultVendorScore()
    {
        var policy = Retry.DefaultPolicy("default-vendor-score");
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

    /// <summary>Puts a default vendor score code in its canonical DVS- form.</summary>
    public static string NormalizeDefaultVendorScoreCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("DVS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"DVS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeDefaultVendorScoreLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
