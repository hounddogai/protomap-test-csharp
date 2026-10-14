namespace Acme.Workforce.Engagement.Tests.Feedback.FeedbackProposal;

using Acme.Workforce.Engagement.Feedback.FeedbackProposal;

/// <summary>Checks of feedback proposal code, run by the project's runner.</summary>
public static class TieredFeedbackProposalViewChecks
{
    public static void TieredFeedbackProposalViewShowsTheStatusInCapitals()
    {
        var item = TieredFeedbackProposal.NewTieredFeedbackProposal("code-15", "name-445");
        var view = TieredFeedbackProposalView.ToTieredFeedbackProposalView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TieredFeedbackProposalView.ToTieredFeedbackProposalViews(new List<TieredFeedbackProposal> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TieredFeedbackProposalViewShowsTheStatusInCapitals();
    }
}
