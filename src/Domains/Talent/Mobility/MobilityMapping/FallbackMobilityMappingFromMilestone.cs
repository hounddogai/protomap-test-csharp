namespace Acme.Workforce.Talent.Mobility.MobilityMapping;

using Acme.Workforce.Talent.Nomination.Milestone;

/// <summary>Functions of fallback mobility mapping from milestone.</summary>
public static class FallbackMobilityMappingFromMilestone
{
    public static Dictionary<string, double> SummarizeWeightedNominationMilestonesForMapping(List<WeightedNominationMilestone> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveLoad();
        }
        return totals;
    }

    public static List<FallbackMobilityMapping> DeriveFallbackMobilityMappingsFromMilestone(List<WeightedNominationMilestone> items)
    {
        var outValue = new List<FallbackMobilityMapping>();
        foreach (var item in items)
        {
            var derived = FallbackMobilityMapping.NewFallbackMobilityMapping(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SplitFallbackMobilityMapping(FallbackMobilityMapping item)
    {
        return GlobalNominationMilestonePolicy.GlobalNominationMilestoneBucket(item.WeightedVolume());
    }
}
