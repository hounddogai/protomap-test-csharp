namespace Acme.Workforce.Engagement.Sentiment.SentimentVariance;

using Acme.Workforce.Engagement.TownHall.Request;

/// <summary>Functions of sentiment agreement from ledger.</summary>
public static class SentimentAgreementFromLedger
{
    public static Dictionary<string, double> SummarizeAdaptiveTownHallLedgersForAgreement(List<AdaptiveTownHallLedger> items)
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

    public static List<SentimentAgreement> DeriveSentimentAgreementsFromLedger(List<AdaptiveTownHallLedger> items)
    {
        var outValue = new List<SentimentAgreement>();
        foreach (var item in items)
        {
            var derived = SentimentAgreement.NewSentimentAgreement(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ApplySentimentAgreement(SentimentAgreement item)
    {
        return BatchedTownHallRequestPolicy.NormalizeBatchedTownHallRequestCode(item.Code);
    }
}
