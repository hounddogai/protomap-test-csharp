namespace Acme.Workforce.Contractors.Offboarding.OffboardingBundle;

using Acme.Workforce.Contractors.Engagement.Ledger;

/// <summary>Functions of legacy offboarding request from entry.</summary>
public static class LegacyOffboardingRequestFromEntry
{
    public static Dictionary<string, double> SummarizeCappedEngagementEntriesForRequest(List<CappedEngagementEntry> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveVolume();
        }
        return totals;
    }

    public static List<LegacyOffboardingRequest> DeriveLegacyOffboardingRequestsFromEntry(List<CappedEngagementEntry> items)
    {
        var outValue = new List<LegacyOffboardingRequest>();
        foreach (var item in items)
        {
            var derived = LegacyOffboardingRequest.NewLegacyOffboardingRequest(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string EstimateLegacyOffboardingRequest(LegacyOffboardingRequest item)
    {
        return CappedEngagementEntryPolicy.ClassifyCappedEngagementEntry(item.ProjectedVolume());
    }
}
