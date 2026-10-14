namespace Acme.Workforce.Engagement.PulseSurvey.OverrideItem;

using Acme.Workforce.Engagement.PulseSurvey.Variance;

/// <summary>Functions of strict pulse survey override from budget.</summary>
public static class StrictPulseSurveyOverrideFromBudget
{
    public static Dictionary<string, double> SummarizePulseSurveyBudgetsForOverride(List<PulseSurveyBudget> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.MeasuredValue();
        }
        return totals;
    }

    public static List<StrictPulseSurveyOverride> DeriveStrictPulseSurveyOverridesFromBudget(List<PulseSurveyBudget> items)
    {
        var outValue = new List<StrictPulseSurveyOverride>();
        foreach (var item in items)
        {
            var derived = StrictPulseSurveyOverride.NewStrictPulseSurveyOverride(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double SummarizeStrictPulseSurveyOverride(StrictPulseSurveyOverride item)
    {
        return PulseSurveyPreferencePolicy.PulseSurveyPreferenceWeight(4, true);
    }
}
