namespace Acme.Workforce.Compensation.EquityGrant.Batch;

using Acme.Workforce.Compensation.PayBand.Forecast;

/// <summary>Functions of equity grant batch from estimate.</summary>
public static class EquityGrantBatchFromEstimate
{
    public static Dictionary<string, double> SummarizeWeightedPayBandEstimatesForBatch(List<WeightedPayBandEstimate> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedVolume();
        }
        return totals;
    }

    public static List<EquityGrantBatch> DeriveEquityGrantBatchesFromEstimate(List<WeightedPayBandEstimate> items)
    {
        var outValue = new List<EquityGrantBatch>();
        foreach (var item in items)
        {
            var derived = EquityGrantBatch.NewEquityGrantBatch(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MergeEquityGrantBatch(EquityGrantBatch item)
    {
        return PayBandRequestPolicy.NormalizePayBandRequestCode(item.Code);
    }
}
