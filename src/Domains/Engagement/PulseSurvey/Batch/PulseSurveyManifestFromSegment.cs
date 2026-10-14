namespace Acme.Workforce.Engagement.PulseSurvey.Batch;

using Acme.Workforce.Engagement.ActionPlan.PlanSegment;

/// <summary>Functions of pulse survey manifest from segment.</summary>
public static class PulseSurveyManifestFromSegment
{
    public static Dictionary<string, double> SummarizeFallbackActionPlanSegmentsForManifest(List<FallbackActionPlanSegment> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetVolume();
        }
        return totals;
    }

    public static List<PulseSurveyManifest> DerivePulseSurveyManifestsFromSegment(List<FallbackActionPlanSegment> items)
    {
        var outValue = new List<PulseSurveyManifest>();
        foreach (var item in items)
        {
            var derived = PulseSurveyManifest.NewPulseSurveyManifest(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string DerivePulseSurveyManifest(PulseSurveyManifest item)
    {
        return FallbackActionPlanSegmentPolicy.NormalizeFallbackActionPlanSegmentCode(item.Code);
    }
}
