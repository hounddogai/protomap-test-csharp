namespace Acme.Workforce.Engagement.PulseSurvey.Profile;

using Acme.Workforce.Engagement.ActionPlan.Draft;

/// <summary>Functions of pulse survey profile from draft.</summary>
public static class PulseSurveyProfileFromDraft
{
    public static Dictionary<string, double> SummarizeFallbackActionPlanDraftsForProfile(List<FallbackActionPlanDraft> items)
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

    public static List<PulseSurveyProfile> DerivePulseSurveyProfilesFromDraft(List<FallbackActionPlanDraft> items)
    {
        var outValue = new List<PulseSurveyProfile>();
        foreach (var item in items)
        {
            var derived = PulseSurveyProfile.NewPulseSurveyProfile(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ApplyPulseSurveyProfile(PulseSurveyProfile item)
    {
        return ScheduledActionPlanAdjustmentPolicy.ScheduledActionPlanAdjustmentWeight(6, true);
    }
}
