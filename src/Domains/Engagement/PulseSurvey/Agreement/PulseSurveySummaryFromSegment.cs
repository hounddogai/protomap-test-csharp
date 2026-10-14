namespace Acme.Workforce.Engagement.PulseSurvey.Agreement;

using Acme.Workforce.Engagement.PulseSurvey.Audit;

/// <summary>Functions of pulse survey summary from segment.</summary>
public static class PulseSurveySummaryFromSegment
{
    public static Dictionary<string, double> SummarizePulseSurveySegmentsForSummary(List<PulseSurveySegment> items)
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

    public static List<PulseSurveySummary> DerivePulseSurveySummariesFromSegment(List<PulseSurveySegment> items)
    {
        var outValue = new List<PulseSurveySummary>();
        foreach (var item in items)
        {
            var derived = PulseSurveySummary.NewPulseSurveySummary(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double RankPulseSurveySummary(PulseSurveySummary item)
    {
        return FallbackPulseSurveySegmentPolicy.FallbackPulseSurveySegmentWeight(1, true);
    }
}
