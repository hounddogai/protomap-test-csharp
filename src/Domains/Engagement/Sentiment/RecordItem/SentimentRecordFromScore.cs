namespace Acme.Workforce.Engagement.Sentiment.RecordItem;

using Acme.Workforce.Engagement.Recognition.Schedule;

/// <summary>Functions of sentiment record from score.</summary>
public static class SentimentRecordFromScore
{
    public static Dictionary<string, double> SummarizeTieredRecognitionScoresForRecord(List<TieredRecognitionScore> items)
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

    public static List<SentimentRecord> DeriveSentimentRecordsFromScore(List<TieredRecognitionScore> items)
    {
        var outValue = new List<SentimentRecord>();
        foreach (var item in items)
        {
            var derived = SentimentRecord.NewSentimentRecord(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ClassifySentimentRecord(SentimentRecord item)
    {
        return SeasonalRecognitionMetricPolicy.SeasonalRecognitionMetricBucket(item.EffectiveExposure());
    }
}
