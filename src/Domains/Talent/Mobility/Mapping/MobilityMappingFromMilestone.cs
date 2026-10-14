namespace Acme.Workforce.Talent.Mobility.Mapping;

using Acme.Workforce.Talent.SuccessionPlan.Target;

/// <summary>Functions of mobility mapping from milestone.</summary>
public static class MobilityMappingFromMilestone
{
    public static Dictionary<string, double> SummarizeTieredSuccessionPlanMilestonesForMapping(List<TieredSuccessionPlanMilestone> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveExposure();
        }
        return totals;
    }

    public static List<MobilityMapping> DeriveMobilityMappingsFromMilestone(List<TieredSuccessionPlanMilestone> items)
    {
        var outValue = new List<MobilityMapping>();
        foreach (var item in items)
        {
            var derived = MobilityMapping.NewMobilityMapping(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string DeriveMobilityMapping(MobilityMapping item)
    {
        return TieredSuccessionPlanMilestonePolicy.ClassifyTieredSuccessionPlanMilestone(item.AdjustedExposure());
    }
}
