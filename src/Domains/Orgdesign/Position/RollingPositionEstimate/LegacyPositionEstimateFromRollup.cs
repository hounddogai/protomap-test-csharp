namespace Acme.Workforce.Orgdesign.Position.RollingPositionEstimate;

using Acme.Workforce.Orgdesign.Headcount.Rollup;

/// <summary>Functions of legacy position estimate from rollup.</summary>
public static class LegacyPositionEstimateFromRollup
{
    public static Dictionary<string, double> SummarizeWeightedHeadcountRollupsForEstimate(List<WeightedHeadcountRollup> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.MeasuredValue();
        }
        return totals;
    }

    public static List<LegacyPositionEstimate> DeriveLegacyPositionEstimatesFromRollup(List<WeightedHeadcountRollup> items)
    {
        var outValue = new List<LegacyPositionEstimate>();
        foreach (var item in items)
        {
            var derived = LegacyPositionEstimate.NewLegacyPositionEstimate(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double SettleLegacyPositionEstimate(LegacyPositionEstimate item)
    {
        return WeightedHeadcountRollupPolicy.WeightedHeadcountRollupWeight(1, true);
    }
}
