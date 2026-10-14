namespace Acme.Workforce.Contractors.Offboarding.Window;

using Acme.Workforce.Contractors.Vendor.VendorTemplate;

/// <summary>Functions of capped offboarding baseline from template.</summary>
public static class CappedOffboardingBaselineFromTemplate
{
    public static Dictionary<string, double> SummarizeBatchedVendorTemplatesForBaseline(List<BatchedVendorTemplate> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedValue();
        }
        return totals;
    }

    public static List<CappedOffboardingBaseline> DeriveCappedOffboardingBaselinesFromTemplate(List<BatchedVendorTemplate> items)
    {
        var outValue = new List<CappedOffboardingBaseline>();
        foreach (var item in items)
        {
            var derived = CappedOffboardingBaseline.NewCappedOffboardingBaseline(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string DeriveCappedOffboardingBaseline(CappedOffboardingBaseline item)
    {
        return PendingVendorManifestPolicy.ClassifyPendingVendorManifest(item.AdjustedLoad());
    }
}
