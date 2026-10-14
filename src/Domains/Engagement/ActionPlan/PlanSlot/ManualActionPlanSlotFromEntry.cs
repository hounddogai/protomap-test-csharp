namespace Acme.Workforce.Engagement.ActionPlan.PlanSlot;

using Acme.Workforce.Engagement.Feedback.FeedbackProposal;

/// <summary>Functions of manual action plan slot from entry.</summary>
public static class ManualActionPlanSlotFromEntry
{
    public static Dictionary<string, double> SummarizeFeedbackEntriesForSlot(List<FeedbackEntry> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedVolume();
        }
        return totals;
    }

    public static List<ManualActionPlanSlot> DeriveManualActionPlanSlotsFromEntry(List<FeedbackEntry> items)
    {
        var outValue = new List<ManualActionPlanSlot>();
        foreach (var item in items)
        {
            var derived = ManualActionPlanSlot.NewManualActionPlanSlot(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SettleManualActionPlanSlot(ManualActionPlanSlot item)
    {
        return LegacyFeedbackProposalPolicy.NormalizeLegacyFeedbackProposalCode(item.Code);
    }
}
