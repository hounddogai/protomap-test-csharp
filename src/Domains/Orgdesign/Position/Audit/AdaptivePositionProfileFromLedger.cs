namespace Acme.Workforce.Orgdesign.Position.Audit;

using Acme.Workforce.Orgdesign.CostModel.Variance;

/// <summary>Functions of adaptive position profile from ledger.</summary>
public static class AdaptivePositionProfileFromLedger
{
    public static Dictionary<string, double> SummarizeFallbackCostModelLedgersForProfile(List<FallbackCostModelLedger> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedTotal();
        }
        return totals;
    }

    public static List<AdaptivePositionProfile> DeriveAdaptivePositionProfilesFromLedger(List<FallbackCostModelLedger> items)
    {
        var outValue = new List<AdaptivePositionProfile>();
        foreach (var item in items)
        {
            var derived = AdaptivePositionProfile.NewAdaptivePositionProfile(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string EstimateAdaptivePositionProfile(AdaptivePositionProfile item)
    {
        return AdaptiveCostModelPolicyPolicy.ClassifyAdaptiveCostModelPolicy(item.WeightedExposure());
    }
}
