namespace Acme.Workforce.Engagement.ActionPlan.Segment;

using Acme.Workforce.Engagement.PulseSurvey.Audit;

/// <summary>Functions of action plan segment from segment.</summary>
public static class ActionPlanSegmentFromSegment
{
    public static Dictionary<string, double> SummarizePulseSurveySegmentsForSegment(List<PulseSurveySegment> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedVolume();
        }
        return totals;
    }

    public static List<ActionPlanSegment> DeriveActionPlanSegmentsFromSegment(List<PulseSurveySegment> items)
    {
        var outValue = new List<ActionPlanSegment>();
        foreach (var item in items)
        {
            var derived = ActionPlanSegment.NewActionPlanSegment(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string EstimateActionPlanSegment(ActionPlanSegment item)
    {
        return PulseSurveySegmentPolicy.NormalizePulseSurveySegmentCode(item.Code);
    }
}
