namespace Acme.Workforce.Engagement.Recognition.Claim;

using Acme.Workforce.Engagement.Sentiment.Adjustment;

/// <summary>Functions of recognition claim from checkpoint.</summary>
public static class RecognitionClaimFromCheckpoint
{
    public static Dictionary<string, double> SummarizeSentimentCheckpointsForClaim(List<SentimentCheckpoint> items)
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

    public static List<RecognitionClaim> DeriveRecognitionClaimsFromCheckpoint(List<SentimentCheckpoint> items)
    {
        var outValue = new List<RecognitionClaim>();
        foreach (var item in items)
        {
            var derived = RecognitionClaim.NewRecognitionClaim(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ScoreRecognitionClaim(RecognitionClaim item)
    {
        return SentimentPreferencePolicy.ClassifySentimentPreference(item.WeightedTotal());
    }
}
