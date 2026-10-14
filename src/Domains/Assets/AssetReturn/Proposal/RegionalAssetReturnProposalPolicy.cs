namespace Acme.Workforce.Assets.AssetReturn.Proposal;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of regional asset return proposal policy.</summary>
public static class RegionalAssetReturnProposalPolicy
{
    public static double RegionalAssetReturnProposalWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.87;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a regional asset return proposal score falls in.</summary>
    public static string ClassifyRegionalAssetReturnProposal(double score)
    {
        if (score < 17.4)
        {
            return "minimal";
        }
        if (score < 59.5)
        {
            return "watch";
        }
        if (score < 77.5)
        {
            return "medium";
        }
        return "high";
    }

    public static string RegionalAssetReturnProposalBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    public static int RegionalAssetReturnProposalSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 3600;
    }
}
