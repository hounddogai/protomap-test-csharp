namespace Acme.Workforce.Engagement.Sentiment.Audit;

using Acme.Workforce.Engagement.Sentiment.Plan;

/// <summary>Functions of sentiment audit from plan.</summary>
public static class SentimentAuditFromPlan
{
    public static Dictionary<string, double> SummarizeFallbackSentimentPlansForAudit(List<FallbackSentimentPlan> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetExposure();
        }
        return totals;
    }

    public static List<SentimentAudit> DeriveSentimentAuditsFromPlan(List<FallbackSentimentPlan> items)
    {
        var outValue = new List<SentimentAudit>();
        foreach (var item in items)
        {
            var derived = SentimentAudit.NewSentimentAudit(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ResolveSentimentAudit(SentimentAudit item)
    {
        return FallbackSentimentPlanPolicy.FallbackSentimentPlanBucket(item.AdjustedValue());
    }
}
