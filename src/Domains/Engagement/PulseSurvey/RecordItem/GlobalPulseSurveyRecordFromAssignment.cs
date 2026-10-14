namespace Acme.Workforce.Engagement.PulseSurvey.RecordItem;

using Acme.Workforce.Engagement.Sentiment.Bundle;

/// <summary>Functions of global pulse survey record from assignment.</summary>
public static class GlobalPulseSurveyRecordFromAssignment
{
    public static Dictionary<string, double> SummarizeAdaptiveSentimentAssignmentsForRecord(List<AdaptiveSentimentAssignment> items)
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

    public static List<GlobalPulseSurveyRecord> DeriveGlobalPulseSurveyRecordsFromAssignment(List<AdaptiveSentimentAssignment> items)
    {
        var outValue = new List<GlobalPulseSurveyRecord>();
        foreach (var item in items)
        {
            var derived = GlobalPulseSurveyRecord.NewGlobalPulseSurveyRecord(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ComputeGlobalPulseSurveyRecord(GlobalPulseSurveyRecord item)
    {
        return AdaptiveSentimentAssignmentPolicy.AdaptiveSentimentAssignmentBucket(item.AdjustedExposure());
    }
}
