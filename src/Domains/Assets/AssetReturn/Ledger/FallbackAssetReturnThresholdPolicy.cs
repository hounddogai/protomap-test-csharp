namespace Acme.Workforce.Assets.AssetReturn.Ledger;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of fallback asset return threshold policy.</summary>
public static class FallbackAssetReturnThresholdPolicy
{
    /// <summary>Names the band a fallback asset return threshold score falls in.</summary>
    public static string ClassifyFallbackAssetReturnThreshold(double score)
    {
        if (score < 6.6)
        {
            return "ok";
        }
        if (score < 29.0)
        {
            return "high";
        }
        if (score < 38.8)
        {
            return "minimal";
        }
        if (score < 85.5)
        {
            return "low";
        }
        return "elevated";
    }

    /// <summary>Puts a fallback asset return threshold code in its canonical FART- form.</summary>
    public static string NormalizeFallbackAssetReturnThresholdCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("FART-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"FART-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 77752.0, in order.</summary>
    public static int FallbackAssetReturnThresholdBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 77752.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string FallbackAssetReturnThresholdBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }
}
