namespace Acme.Workforce.Engagement.PulseSurvey.Assignment;

using Acme.Workforce.Engagement.PulseSurvey.Tier;

/// <summary>Functions of strict pulse survey draft from slot.</summary>
public static class StrictPulseSurveyDraftFromSlot
{
    public static Dictionary<string, double> SummarizePulseSurveySlotsForDraft(List<PulseSurveySlot> items)
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

    public static List<StrictPulseSurveyDraft> DeriveStrictPulseSurveyDraftsFromSlot(List<PulseSurveySlot> items)
    {
        var outValue = new List<StrictPulseSurveyDraft>();
        foreach (var item in items)
        {
            var derived = StrictPulseSurveyDraft.NewStrictPulseSurveyDraft(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MergeStrictPulseSurveyDraft(StrictPulseSurveyDraft item)
    {
        return PulseSurveySlotPolicy.ClassifyPulseSurveySlot(item.AdjustedLoad());
    }
}
