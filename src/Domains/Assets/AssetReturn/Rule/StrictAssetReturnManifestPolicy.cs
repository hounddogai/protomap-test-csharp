namespace Acme.Workforce.Assets.AssetReturn.Rule;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of strict asset return manifest policy.</summary>
public static class StrictAssetReturnManifestPolicy
{
    /// <summary>Puts a strict asset return manifest code in its canonical SARM- form.</summary>
    public static string NormalizeStrictAssetReturnManifestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SARM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SARM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int StrictAssetReturnManifestSpan(int start, int end)
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
    public static int ProbeStrictAssetReturnManifest()
    {
        var policy = Retry.DefaultPolicy("strict-asset-return-manifest");
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

    public static double StrictAssetReturnManifestWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 0.6;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
