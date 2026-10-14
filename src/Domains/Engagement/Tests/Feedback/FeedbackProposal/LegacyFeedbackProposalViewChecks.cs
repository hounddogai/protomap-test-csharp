namespace Acme.Workforce.Engagement.Tests.Feedback.FeedbackProposal;

using Acme.Workforce.Engagement.Feedback.FeedbackProposal;

/// <summary>Checks of feedback proposal code, run by the project's runner.</summary>
public static class LegacyFeedbackProposalViewChecks
{
    public static void LegacyFeedbackProposalViewShowsTheStatusInCapitals()
    {
        var item = LegacyFeedbackProposal.NewLegacyFeedbackProposal("code-195", "label-553");
        var view = LegacyFeedbackProposalView.ToLegacyFeedbackProposalView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LegacyFeedbackProposalView.ToLegacyFeedbackProposalViews(new List<LegacyFeedbackProposal> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LegacyFeedbackProposalViewShowsTheStatusInCapitals();
    }
}
