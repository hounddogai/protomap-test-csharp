namespace Acme.Workforce.Engagement.TownHall.HallRule;

using Acme.Workforce.Engagement.Sentiment.SentimentSummary;

/// <summary>Functions of town hall record from profile.</summary>
public static class TownHallRecordFromProfile
{
    public static Dictionary<string, double> SummarizeFallbackSentimentProfilesForRecord(List<FallbackSentimentProfile> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveVolume();
        }
        return totals;
    }

    public static List<TownHallRecord> DeriveTownHallRecordsFromProfile(List<FallbackSentimentProfile> items)
    {
        var outValue = new List<TownHallRecord>();
        foreach (var item in items)
        {
            var derived = TownHallRecord.NewTownHallRecord(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string PlanTownHallRecord(TownHallRecord item)
    {
        return TieredSentimentSummaryPolicy.TieredSentimentSummaryBucket(item.WeightedValue());
    }
}
