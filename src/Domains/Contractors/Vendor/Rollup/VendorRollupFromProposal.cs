namespace Acme.Workforce.Contractors.Vendor.Rollup;

using Acme.Workforce.Contractors.Engagement.Proposal;

/// <summary>Functions of vendor rollup from proposal.</summary>
public static class VendorRollupFromProposal
{
    public static Dictionary<string, double> SummarizeGlobalEngagementProposalsForRollup(List<GlobalEngagementProposal> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveLoad();
        }
        return totals;
    }

    public static List<VendorRollup> DeriveVendorRollupsFromProposal(List<GlobalEngagementProposal> items)
    {
        var outValue = new List<VendorRollup>();
        foreach (var item in items)
        {
            var derived = VendorRollup.NewVendorRollup(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SplitVendorRollup(VendorRollup item)
    {
        return EngagementAllocationPolicy.EngagementAllocationBucket(item.MeasuredValue());
    }
}
