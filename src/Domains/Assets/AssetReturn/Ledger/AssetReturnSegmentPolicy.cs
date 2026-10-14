namespace Acme.Workforce.Assets.AssetReturn.Ledger;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of asset return segment policy.</summary>
public static class AssetReturnSegmentPolicy
{
    /// <summary>Names the band a asset return segment score falls in.</summary>
    public static string ClassifyAssetReturnSegment(double score)
    {
        if (score < 14.3)
        {
            return "ok";
        }
        if (score < 48.9)
        {
            return "low";
        }
        if (score < 49.3)
        {
            return "elevated";
        }
        return "high";
    }

    public static int AssetReturnSegmentSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 86400;
    }

    public static string AssetReturnSegmentBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    /// <summary>Puts a asset return segment code in its canonical ARS- form.</summary>
    public static string NormalizeAssetReturnSegmentCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("ARS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"ARS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
