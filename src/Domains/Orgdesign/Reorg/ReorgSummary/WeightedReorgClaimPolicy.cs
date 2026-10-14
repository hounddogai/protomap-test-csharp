namespace Acme.Workforce.Orgdesign.Reorg.ReorgSummary;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of weighted reorg claim policy.</summary>
public static class WeightedReorgClaimPolicy
{
    /// <summary>Puts a weighted reorg claim code in its canonical WRC- form.</summary>
    public static string NormalizeWeightedReorgClaimCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("WRC-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"WRC-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int WeightedReorgClaimSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 300)
        {
            return 0;
        }
        return span / 3600;
    }

    public static string WeightedReorgClaimBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }
}
