namespace Acme.Workforce.Engagement.ActionPlan.RegionalPlanPreference;

using Acme.Workforce.Engagement.PulseSurvey.Quota;

/// <summary>Functions of manual action plan preference from quota.</summary>
public static class ManualActionPlanPreferenceFromQuota
{
    public static Dictionary<string, double> SummarizeManualPulseSurveyQuotasForPreference(List<ManualPulseSurveyQuota> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedValue();
        }
        return totals;
    }

    public static List<ManualActionPlanPreference> DeriveManualActionPlanPreferencesFromQuota(List<ManualPulseSurveyQuota> items)
    {
        var outValue = new List<ManualActionPlanPreference>();
        foreach (var item in items)
        {
            var derived = ManualActionPlanPreference.NewManualActionPlanPreference(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ResolveManualActionPlanPreference(ManualActionPlanPreference item)
    {
        return ManualPulseSurveyQuotaPolicy.ClassifyManualPulseSurveyQuota(item.WeightedTotal());
    }
}
