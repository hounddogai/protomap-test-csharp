namespace Acme.Workforce.Contractors.Rate.Milestone;

using Acme.Workforce.Contractors.Vendor.Batch;

/// <summary>Functions of rate baseline from batch.</summary>
public static class RateBaselineFromBatch
{
    public static Dictionary<string, double> SummarizeVendorBatchesForBaseline(List<VendorBatch> items)
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

    public static List<RateBaseline> DeriveRateBaselinesFromBatch(List<VendorBatch> items)
    {
        var outValue = new List<RateBaseline>();
        foreach (var item in items)
        {
            var derived = RateBaseline.NewRateBaseline(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double MatchRateBaseline(RateBaseline item)
    {
        return VendorBatchPolicy.VendorBatchWeight(6, true);
    }
}
