namespace Acme.Workforce.Contractors.Vendor.Template;

using Acme.Workforce.Contractors.Rate.Cycle;

/// <summary>Functions of vendor template from request.</summary>
public static class VendorTemplateFromRequest
{
    public static Dictionary<string, double> SummarizeWeightedRateRequestsForTemplate(List<WeightedRateRequest> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedExposure();
        }
        return totals;
    }

    public static List<VendorTemplate> DeriveVendorTemplatesFromRequest(List<WeightedRateRequest> items)
    {
        var outValue = new List<VendorTemplate>();
        foreach (var item in items)
        {
            var derived = VendorTemplate.NewVendorTemplate(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string EstimateVendorTemplate(VendorTemplate item)
    {
        return AdaptiveRateTemplatePolicy.AdaptiveRateTemplateBucket(item.NetValue());
    }
}
