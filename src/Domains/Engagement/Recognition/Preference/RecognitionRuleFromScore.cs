namespace Acme.Workforce.Engagement.Recognition.Preference;

using Acme.Workforce.Engagement.Feedback.Claim;

/// <summary>Functions of recognition rule from score.</summary>
public static class RecognitionRuleFromScore
{
    public static Dictionary<string, double> SummarizeAdaptiveFeedbackScoresForRule(List<AdaptiveFeedbackScore> items)
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

    public static List<RecognitionRule> DeriveRecognitionRulesFromScore(List<AdaptiveFeedbackScore> items)
    {
        var outValue = new List<RecognitionRule>();
        foreach (var item in items)
        {
            var derived = RecognitionRule.NewRecognitionRule(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ApplyRecognitionRule(RecognitionRule item)
    {
        return PrimaryFeedbackClaimPolicy.PrimaryFeedbackClaimWeight(7, true);
    }
}
