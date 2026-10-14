namespace Acme.Workforce.Assets.AssetReturn.Score;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of batched asset return score policy.</summary>
public static class BatchedAssetReturnScorePolicy
{
    public static string BatchedAssetReturnScoreBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    /// <summary>Puts a batched asset return score code in its canonical BARS- form.</summary>
    public static string NormalizeBatchedAssetReturnScoreCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("BARS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"BARS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
