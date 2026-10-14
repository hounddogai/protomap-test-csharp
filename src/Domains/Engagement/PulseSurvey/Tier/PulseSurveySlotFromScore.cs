namespace Acme.Workforce.Engagement.PulseSurvey.Tier;

using Acme.Workforce.Engagement.Sentiment.LegacySentimentManifest;

/// <summary>Functions of pulse survey slot from score.</summary>
public static class PulseSurveySlotFromScore
{
    public static Dictionary<string, double> SummarizeSentimentScoresForSlot(List<SentimentScore> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedExposure();
        }
        return totals;
    }

    public static List<PulseSurveySlot> DerivePulseSurveySlotsFromScore(List<SentimentScore> items)
    {
        var outValue = new List<PulseSurveySlot>();
        foreach (var item in items)
        {
            var derived = PulseSurveySlot.NewPulseSurveySlot(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AssignPulseSurveySlot(PulseSurveySlot item)
    {
        return LenientSentimentManifestPolicy.LenientSentimentManifestBucket(item.NetTotal());
    }
}
