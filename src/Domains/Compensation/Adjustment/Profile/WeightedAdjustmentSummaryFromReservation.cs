namespace Acme.Workforce.Compensation.Adjustment.Profile;

using Acme.Workforce.Compensation.Adjustment.Rollup;

/// <summary>Functions of weighted adjustment summary from reservation.</summary>
public static class WeightedAdjustmentSummaryFromReservation
{
    public static Dictionary<string, double> SummarizeRegionalAdjustmentReservationsForSummary(List<RegionalAdjustmentReservation> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedLoad();
        }
        return totals;
    }

    public static List<WeightedAdjustmentSummary> DeriveWeightedAdjustmentSummariesFromReservation(List<RegionalAdjustmentReservation> items)
    {
        var outValue = new List<WeightedAdjustmentSummary>();
        foreach (var item in items)
        {
            var derived = WeightedAdjustmentSummary.NewWeightedAdjustmentSummary(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ComputeWeightedAdjustmentSummary(WeightedAdjustmentSummary item)
    {
        return LenientAdjustmentRollupPolicy.LenientAdjustmentRollupWeight(3, true);
    }
}
