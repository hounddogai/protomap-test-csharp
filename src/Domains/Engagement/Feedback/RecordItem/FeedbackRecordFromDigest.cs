namespace Acme.Workforce.Engagement.Feedback.RecordItem;

using Acme.Workforce.Engagement.Recognition.Digest;

/// <summary>Functions of feedback record from digest.</summary>
public static class FeedbackRecordFromDigest
{
    public static Dictionary<string, double> SummarizeRecognitionDigestsForRecord(List<RecognitionDigest> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetValue();
        }
        return totals;
    }

    public static List<FeedbackRecord> DeriveFeedbackRecordsFromDigest(List<RecognitionDigest> items)
    {
        var outValue = new List<FeedbackRecord>();
        foreach (var item in items)
        {
            var derived = FeedbackRecord.NewFeedbackRecord(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ScoreFeedbackRecord(FeedbackRecord item)
    {
        return RecognitionDigestPolicy.ClassifyRecognitionDigest(item.WeightedVolume());
    }
}
