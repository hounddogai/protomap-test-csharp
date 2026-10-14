namespace Acme.Workforce.Contractors.Vendor.Entry;

using Acme.Workforce.Contractors.Offboarding.Window;

/// <summary>Functions of legacy vendor batch from baseline.</summary>
public static class LegacyVendorBatchFromBaseline
{
    public static Dictionary<string, double> SummarizeCappedOffboardingBaselinesForBatch(List<CappedOffboardingBaseline> items)
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

    public static List<LegacyVendorBatch> DeriveLegacyVendorBatchesFromBaseline(List<CappedOffboardingBaseline> items)
    {
        var outValue = new List<LegacyVendorBatch>();
        foreach (var item in items)
        {
            var derived = LegacyVendorBatch.NewLegacyVendorBatch(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string RankLegacyVendorBatch(LegacyVendorBatch item)
    {
        return CappedOffboardingBaselinePolicy.ClassifyCappedOffboardingBaseline(item.NetValue());
    }
}
