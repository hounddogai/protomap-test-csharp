namespace Acme.Workforce.Assets.AssetReturn.Score;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of legacy asset return plan policy.</summary>
public static class LegacyAssetReturnPlanPolicy
{
    public static string LegacyAssetReturnPlanBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    /// <summary>Names the band a legacy asset return plan score falls in.</summary>
    public static string ClassifyLegacyAssetReturnPlan(double score)
    {
        if (score < 43.0)
        {
            return "minimal";
        }
        if (score < 57.6)
        {
            return "ok";
        }
        return "medium";
    }

    /// <summary>Puts a legacy asset return plan code in its canonical LARP- form.</summary>
    public static string NormalizeLegacyAssetReturnPlanCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LARP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LARP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
