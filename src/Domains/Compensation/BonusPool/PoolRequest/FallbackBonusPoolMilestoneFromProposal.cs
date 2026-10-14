namespace Acme.Workforce.Compensation.BonusPool.PoolRequest;

using Acme.Workforce.Compensation.BonusPool.PoolTier;

/// <summary>Functions of fallback bonus pool milestone from proposal.</summary>
public static class FallbackBonusPoolMilestoneFromProposal
{
    public static Dictionary<string, double> SummarizeLegacyBonusPoolProposalsForMilestone(List<LegacyBonusPoolProposal> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedVolume();
        }
        return totals;
    }

    public static List<FallbackBonusPoolMilestone> DeriveFallbackBonusPoolMilestonesFromProposal(List<LegacyBonusPoolProposal> items)
    {
        var outValue = new List<FallbackBonusPoolMilestone>();
        foreach (var item in items)
        {
            var derived = FallbackBonusPoolMilestone.NewFallbackBonusPoolMilestone(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double SplitFallbackBonusPoolMilestone(FallbackBonusPoolMilestone item)
    {
        return RollingBonusPoolSlotPolicy.RollingBonusPoolSlotWeight(2, true);
    }
}
