namespace Acme.Workforce.Engagement.ActionPlan.WeightedPlanSegment;

using Acme.Workforce.Engagement.PulseSurvey.Batch;

/// <summary>Functions of global action plan entry from manifest.</summary>
public static class GlobalActionPlanEntryFromManifest
{
    public static Dictionary<string, double> SummarizePulseSurveyManifestsForEntry(List<PulseSurveyManifest> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetTotal();
        }
        return totals;
    }

    public static List<GlobalActionPlanEntry> DeriveGlobalActionPlanEntriesFromManifest(List<PulseSurveyManifest> items)
    {
        var outValue = new List<GlobalActionPlanEntry>();
        foreach (var item in items)
        {
            var derived = GlobalActionPlanEntry.NewGlobalActionPlanEntry(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string DeriveGlobalActionPlanEntry(GlobalActionPlanEntry item)
    {
        return PulseSurveyBatchPolicy.ClassifyPulseSurveyBatch(item.AdjustedValue());
    }
}
