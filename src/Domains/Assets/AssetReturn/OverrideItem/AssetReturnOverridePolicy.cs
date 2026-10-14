namespace Acme.Workforce.Assets.AssetReturn.OverrideItem;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of asset return override policy.</summary>
public static class AssetReturnOverridePolicy
{
    /// <summary>Puts a asset return override code in its canonical ARO- form.</summary>
    public static string NormalizeAssetReturnOverrideCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("ARO-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"ARO-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeAssetReturnOverride()
    {
        var policy = Retry.DefaultPolicy("asset-return-override");
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
