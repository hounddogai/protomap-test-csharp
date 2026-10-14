namespace Acme.Workforce.Engagement.Tests.Feedback.Rule;

using Acme.Workforce.Engagement.Feedback.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class LegacyFeedbackClaimViewChecks
{
    public static void LegacyFeedbackClaimViewShowsTheStatusInCapitals()
    {
        var item = LegacyFeedbackClaim.NewLegacyFeedbackClaim("code-454", "label-764");
        var view = LegacyFeedbackClaimView.ToLegacyFeedbackClaimView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LegacyFeedbackClaimView.ToLegacyFeedbackClaimViews(new List<LegacyFeedbackClaim> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LegacyFeedbackClaimViewShowsTheStatusInCapitals();
    }
}
