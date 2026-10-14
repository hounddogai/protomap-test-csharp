namespace Acme.Workforce.Compensation.EquityGrant.Plan;

using Acme.Workforce.Compensation.PayBand.Reservation;

/// <summary>Functions of batched equity grant preference from group.</summary>
public static class BatchedEquityGrantPreferenceFromGroup
{
    public static Dictionary<string, double> SummarizeDefaultPayBandGroupsForPreference(List<DefaultPayBandGroup> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedLoad();
        }
        return totals;
    }

    public static List<BatchedEquityGrantPreference> DeriveBatchedEquityGrantPreferencesFromGroup(List<DefaultPayBandGroup> items)
    {
        var outValue = new List<BatchedEquityGrantPreference>();
        foreach (var item in items)
        {
            var derived = BatchedEquityGrantPreference.NewBatchedEquityGrantPreference(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ResolveBatchedEquityGrantPreference(BatchedEquityGrantPreference item)
    {
        return TieredPayBandEstimatePolicy.ClassifyTieredPayBandEstimate(item.WeightedLoad());
    }
}
