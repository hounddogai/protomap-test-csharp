namespace Acme.Workforce.Contractors.Rate.Proposal;

using Acme.Workforce.Contractors.Vendor.Segment;

/// <summary>Functions of tiered rate request from quota.</summary>
public static class TieredRateRequestFromQuota
{
    public static Dictionary<string, double> SummarizeVendorQuotasForRequest(List<VendorQuota> items)
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

    public static List<TieredRateRequest> DeriveTieredRateRequestsFromQuota(List<VendorQuota> items)
    {
        var outValue = new List<TieredRateRequest>();
        foreach (var item in items)
        {
            var derived = TieredRateRequest.NewTieredRateRequest(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ApplyTieredRateRequest(TieredRateRequest item)
    {
        return VendorBaselinePolicy.NormalizeVendorBaselineCode(item.Code);
    }
}
