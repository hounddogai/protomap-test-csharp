namespace Acme.Workforce.Engagement.Feedback.Preference;

using Acme.Workforce.Engagement.PulseSurvey.Claim;

/// <summary>Functions of feedback preference from claim.</summary>
public static class FeedbackPreferenceFromClaim
{
    public static Dictionary<string, double> SummarizePendingPulseSurveyClaimsForPreference(List<PendingPulseSurveyClaim> items)
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

    public static List<FeedbackPreference> DeriveFeedbackPreferencesFromClaim(List<PendingPulseSurveyClaim> items)
    {
        var outValue = new List<FeedbackPreference>();
        foreach (var item in items)
        {
            var derived = FeedbackPreference.NewFeedbackPreference(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ResolveFeedbackPreference(FeedbackPreference item)
    {
        return PendingPulseSurveyClaimPolicy.PendingPulseSurveyClaimWeight(7, true);
    }
}
