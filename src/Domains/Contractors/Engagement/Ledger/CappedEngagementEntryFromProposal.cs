namespace Acme.Workforce.Contractors.Engagement.Ledger;

using Acme.Workforce.Contractors.Engagement.Proposal;

/// <summary>Functions of capped engagement entry from proposal.</summary>
public static class CappedEngagementEntryFromProposal
{
    public static Dictionary<string, double> SummarizeGlobalEngagementProposalsForEntry(List<GlobalEngagementProposal> items)
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

    public static List<CappedEngagementEntry> DeriveCappedEngagementEntriesFromProposal(List<GlobalEngagementProposal> items)
    {
        var outValue = new List<CappedEngagementEntry>();
        foreach (var item in items)
        {
            var derived = CappedEngagementEntry.NewCappedEngagementEntry(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SplitCappedEngagementEntry(CappedEngagementEntry item)
    {
        return EngagementAllocationPolicy.EngagementAllocationBucket(item.EffectiveVolume());
    }
}
