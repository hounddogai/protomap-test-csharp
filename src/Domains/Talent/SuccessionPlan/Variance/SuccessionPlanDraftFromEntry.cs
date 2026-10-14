namespace Acme.Workforce.Talent.SuccessionPlan.Variance;

using Acme.Workforce.Talent.Mobility.Entry;

/// <summary>Functions of succession plan draft from entry.</summary>
public static class SuccessionPlanDraftFromEntry
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

    public static List<SuccessionPlanDraft> DeriveSuccessionPlanDraftsFromEntry(List<MobilityEntry> items)
    {
        var outValue = new List<SuccessionPlanDraft>();
        foreach (var item in items)
        {
            var derived = SuccessionPlanDraft.NewSuccessionPlanDraft(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MatchSuccessionPlanDraft(SuccessionPlanDraft item)
    {
        return BatchedMobilityReservationPolicy.ClassifyBatchedMobilityReservation(item.AdjustedExposure());
    }
}
