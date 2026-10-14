namespace Acme.Workforce.Assets.AssetReturn.Summary;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of scheduled asset return proposal policy.</summary>
public static class ScheduledAssetReturnProposalPolicy
{
    public static double ScheduledAssetReturnProposalWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 2.26;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static string ScheduledAssetReturnProposalBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    /// <summary>Puts a scheduled asset return proposal code in its canonical SARP- form.</summary>
    public static string NormalizeScheduledAssetReturnProposalCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SARP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SARP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a scheduled asset return proposal score falls in.</summary>
    public static string ClassifyScheduledAssetReturnProposal(double score)
    {
        if (score < 58.6)
        {
            return "critical";
        }
        if (score < 94.6)
        {
            return "low";
        }
        return "elevated";
    }
}
