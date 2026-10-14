namespace Acme.Workforce.Engagement.PulseSurvey.Group;

using Acme.Workforce.Engagement.ActionPlan.Request;

/// <summary>Functions of pulse survey adjustment from manifest.</summary>
public static class PulseSurveyAdjustmentFromManifest
{
    public static Dictionary<string, double> SummarizeActionPlanManifestsForAdjustment(List<ActionPlanManifest> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedLoad();
        }
        return totals;
    }

    public static List<PulseSurveyAdjustment> DerivePulseSurveyAdjustmentsFromManifest(List<ActionPlanManifest> items)
    {
        var outValue = new List<PulseSurveyAdjustment>();
        foreach (var item in items)
        {
            var derived = PulseSurveyAdjustment.NewPulseSurveyAdjustment(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MatchPulseSurveyAdjustment(PulseSurveyAdjustment item)
    {
        return ActionPlanManifestPolicy.NormalizeActionPlanManifestCode(item.Code);
    }
}
