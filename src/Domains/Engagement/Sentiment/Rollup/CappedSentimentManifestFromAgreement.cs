namespace Acme.Workforce.Engagement.Sentiment.Rollup;

using Acme.Workforce.Engagement.Sentiment.SentimentVariance;

/// <summary>Functions of capped sentiment manifest from agreement.</summary>
public static class CappedSentimentManifestFromAgreement
{
    public static Dictionary<string, double> SummarizeSentimentAgreementsForManifest(List<SentimentAgreement> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.MeasuredValue();
        }
        return totals;
    }

    public static List<CappedSentimentManifest> DeriveCappedSentimentManifestsFromAgreement(List<SentimentAgreement> items)
    {
        var outValue = new List<CappedSentimentManifest>();
        foreach (var item in items)
        {
            var derived = CappedSentimentManifest.NewCappedSentimentManifest(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ReconcileCappedSentimentManifest(CappedSentimentManifest item)
    {
        return SentimentAgreementPolicy.NormalizeSentimentAgreementCode(item.Code);
    }
}
