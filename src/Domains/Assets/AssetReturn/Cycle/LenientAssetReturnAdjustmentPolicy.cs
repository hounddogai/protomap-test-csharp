namespace Acme.Workforce.Assets.AssetReturn.Cycle;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of lenient asset return adjustment policy.</summary>
public static class LenientAssetReturnAdjustmentPolicy
{
    /// <summary>Names the band a lenient asset return adjustment score falls in.</summary>
    public static string ClassifyLenientAssetReturnAdjustment(double score)
    {
        if (score < 16.9)
        {
            return "critical";
        }
        if (score < 66.5)
        {
            return "minimal";
        }
        if (score < 96.5)
        {
            return "elevated";
        }
        if (score < 97.5)
        {
            return "high";
        }
        return "ok";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeLenientAssetReturnAdjustment()
    {
        var policy = Retry.DefaultPolicy("lenient-asset-return-adjustment");
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

    public static string LenientAssetReturnAdjustmentBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    /// <summary>Puts a lenient asset return adjustment code in its canonical LARA- form.</summary>
    public static string NormalizeLenientAssetReturnAdjustmentCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LARA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LARA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
