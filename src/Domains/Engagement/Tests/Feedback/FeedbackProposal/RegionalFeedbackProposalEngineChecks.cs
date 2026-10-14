namespace Acme.Workforce.Engagement.Tests.Feedback.FeedbackProposal;

using Acme.Workforce.Engagement.Feedback.FeedbackProposal;

/// <summary>Checks of feedback proposal code, run by the project's runner.</summary>
public static class RegionalFeedbackProposalEngineChecks
{
    public static void RegionalFeedbackProposalEngineKeepsResultsWithinBounds()
    {
        var engine = RegionalFeedbackProposalEngine.DefaultRegionalFeedbackProposalEngine();
        var item = RegionalFeedbackProposal.NewRegionalFeedbackProposal("code-184", "label-33");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RegionalFeedbackProposalEngineKeepsResultsWithinBounds();
    }
}
