namespace Acme.Workforce.Talent.Nomination.Rule;

using Acme.Workforce.Talent.Mobility.Entry;

/// <summary>Functions of nomination draft from entry.</summary>
public static class NominationDraftFromEntry
{
    public static Dictionary<string, double> SummarizeMobilityEntriesForDraft(List<MobilityEntry> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedTotal();
        }
        return totals;
    }

    public static List<NominationDraft> DeriveNominationDraftsFromEntry(List<MobilityEntry> items)
    {
        var outValue = new List<NominationDraft>();
        foreach (var item in items)
        {
            var derived = NominationDraft.NewNominationDraft(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SplitNominationDraft(NominationDraft item)
    {
        return BatchedMobilityReservationPolicy.ClassifyBatchedMobilityReservation(item.WeightedValue());
    }
}
