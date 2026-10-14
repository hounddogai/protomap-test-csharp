namespace Acme.Workforce.Engagement.Sentiment.Rollup;

using Acme.Workforce.Engagement.PulseSurvey.Profile;

/// <summary>Functions of manual sentiment adjustment from milestone.</summary>
public static class ManualSentimentAdjustmentFromMilestone
{
    public static Dictionary<string, double> SummarizePulseSurveyMilestonesForAdjustment(List<PulseSurveyMilestone> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedVolume();
        }
        return totals;
    }

    public static List<ManualSentimentAdjustment> DeriveManualSentimentAdjustmentsFromMilestone(List<PulseSurveyMilestone> items)
    {
        var outValue = new List<ManualSentimentAdjustment>();
        foreach (var item in items)
        {
            var derived = ManualSentimentAdjustment.NewManualSentimentAdjustment(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ResolveManualSentimentAdjustment(ManualSentimentAdjustment item)
    {
        return SeasonalPulseSurveyAuditPolicy.SeasonalPulseSurveyAuditBucket(item.NetTotal());
    }
}
