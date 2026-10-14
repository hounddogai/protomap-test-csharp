namespace Acme.Workforce.Engagement.TownHall.Target;

using Acme.Workforce.Engagement.Sentiment.Audit;

/// <summary>Functions of legacy town hall summary from request.</summary>
public static class LegacyTownHallSummaryFromRequest
{
    public static Dictionary<string, double> SummarizeSentimentRequestsForSummary(List<SentimentRequest> items)
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

    public static List<LegacyTownHallSummary> DeriveLegacyTownHallSummariesFromRequest(List<SentimentRequest> items)
    {
        var outValue = new List<LegacyTownHallSummary>();
        foreach (var item in items)
        {
            var derived = LegacyTownHallSummary.NewLegacyTownHallSummary(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string DeriveLegacyTownHallSummary(LegacyTownHallSummary item)
    {
        return SentimentAuditPolicy.NormalizeSentimentAuditCode(item.Code);
    }
}
