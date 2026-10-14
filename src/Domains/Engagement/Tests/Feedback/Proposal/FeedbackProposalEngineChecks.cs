namespace Acme.Workforce.Engagement.Tests.Feedback.Proposal;

using Acme.Workforce.Engagement.Feedback.Proposal;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class FeedbackProposalEngineChecks
{
    public static void FeedbackProposalEngineKeepsResultsWithinBounds()
    {
        var engine = FeedbackProposalEngine.DefaultFeedbackProposalEngine(Flags.NewRegistry());
        var item = FeedbackProposal.NewFeedbackProposal("code-123", "name-943");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        FeedbackProposalEngineKeepsResultsWithinBounds();
    }
}
