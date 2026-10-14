namespace Acme.Workforce.Contractors.Vendor.Batch;

using Acme.Workforce.Contractors.Rate.Mapping;

/// <summary>Functions of vendor batch from preference.</summary>
public static class VendorBatchFromPreference
{
    public static Dictionary<string, double> SummarizeLegacyRatePreferencesForBatch(List<LegacyRatePreference> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveTotal();
        }
        return totals;
    }

    public static List<VendorBatch> DeriveVendorBatchesFromPreference(List<LegacyRatePreference> items)
    {
        var outValue = new List<VendorBatch>();
        foreach (var item in items)
        {
            var derived = VendorBatch.NewVendorBatch(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string BalanceVendorBatch(VendorBatch item)
    {
        return StrictRateMappingPolicy.ClassifyStrictRateMapping(item.MeasuredValue());
    }
}
