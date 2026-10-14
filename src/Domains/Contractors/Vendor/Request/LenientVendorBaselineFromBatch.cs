namespace Acme.Workforce.Contractors.Vendor.Request;

using Acme.Workforce.Contractors.Vendor.Batch;

/// <summary>Functions of lenient vendor baseline from batch.</summary>
public static class LenientVendorBaselineFromBatch
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

    public static List<LenientVendorBaseline> DeriveLenientVendorBaselinesFromBatch(List<VendorBatch> items)
    {
        var outValue = new List<LenientVendorBaseline>();
        foreach (var item in items)
        {
            var derived = LenientVendorBaseline.NewLenientVendorBaseline(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string DeriveLenientVendorBaseline(LenientVendorBaseline item)
    {
        return VendorExceptionPolicy.NormalizeVendorExceptionCode(item.Code);
    }
}
