namespace Acme.Workforce.Compensation.BonusPool.Window;

using Acme.Workforce.Compensation.MeritCycle.CycleRollup;

/// <summary>Functions of adaptive bonus pool estimate from rollup.</summary>
public static class AdaptiveBonusPoolEstimateFromRollup
{
    public static Dictionary<string, double> SummarizeSeasonalMeritCycleRollupsForEstimate(List<SeasonalMeritCycleRollup> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedLoad();
        }
        return totals;
    }

    public static List<AdaptiveBonusPoolEstimate> DeriveAdaptiveBonusPoolEstimatesFromRollup(List<SeasonalMeritCycleRollup> items)
    {
        var outValue = new List<AdaptiveBonusPoolEstimate>();
        foreach (var item in items)
        {
            var derived = AdaptiveBonusPoolEstimate.NewAdaptiveBonusPoolEstimate(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SplitAdaptiveBonusPoolEstimate(AdaptiveBonusPoolEstimate item)
    {
        return MeritCyclePolicyPolicy.NormalizeMeritCyclePolicyCode(item.Code);
    }
}
