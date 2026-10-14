namespace Acme.Workforce.Contractors.Engagement.Proposal;

using Acme.Workforce.Contractors.Vendor.Entry;

/// <summary>Functions of global engagement proposal from batch.</summary>
public static class GlobalEngagementProposalFromBatch
{
    public static Dictionary<string, double> SummarizeLegacyVendorBatchesForProposal(List<LegacyVendorBatch> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetValue();
        }
        return totals;
    }

    public static List<GlobalEngagementProposal> DeriveGlobalEngagementProposalsFromBatch(List<LegacyVendorBatch> items)
    {
        var outValue = new List<GlobalEngagementProposal>();
        foreach (var item in items)
        {
            var derived = GlobalEngagementProposal.NewGlobalEngagementProposal(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ClassifyGlobalEngagementProposal(GlobalEngagementProposal item)
    {
        return CappedVendorEntryPolicy.ClassifyCappedVendorEntry(item.EffectiveLoad());
    }
}
