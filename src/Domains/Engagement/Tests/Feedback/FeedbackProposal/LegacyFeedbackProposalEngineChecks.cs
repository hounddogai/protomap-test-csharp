namespace Acme.Workforce.Engagement.Tests.Feedback.FeedbackProposal;

using Acme.Workforce.Engagement.Feedback.FeedbackProposal;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of feedback proposal code, run by the project's runner.</summary>
public static class LegacyFeedbackProposalEngineChecks
{
    public static void LegacyFeedbackProposalEngineKeepsResultsWithinBounds()
    {
        var engine = LegacyFeedbackProposalEngine.DefaultLegacyFeedbackProposalEngine(Flags.NewRegistry());
        var item = LegacyFeedbackProposal.NewLegacyFeedbackProposal("code-195", "label-553");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LegacyFeedbackProposalEngineKeepsResultsWithinBounds();
    }
}
