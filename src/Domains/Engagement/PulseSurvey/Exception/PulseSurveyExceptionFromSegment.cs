namespace Acme.Workforce.Engagement.PulseSurvey.Exception;

using Acme.Workforce.Engagement.ActionPlan.WeightedPlanSegment;

/// <summary>Functions of pulse survey exception from segment.</summary>
public static class PulseSurveyExceptionFromSegment
{
    public static Dictionary<string, double> SummarizeGlobalActionPlanSegmentsForException(List<GlobalActionPlanSegment> items)
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

    public static List<PulseSurveyException> DerivePulseSurveyExceptionsFromSegment(List<GlobalActionPlanSegment> items)
    {
        var outValue = new List<PulseSurveyException>();
        foreach (var item in items)
        {
            var derived = PulseSurveyException.NewPulseSurveyException(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SplitPulseSurveyException(PulseSurveyException item)
    {
        return GlobalActionPlanSegmentPolicy.ClassifyGlobalActionPlanSegment(item.AdjustedVolume());
    }
}
