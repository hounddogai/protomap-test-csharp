namespace Acme.Workforce.Engagement.Tests.Feedback.Group;

using Acme.Workforce.Engagement.Feedback.Group;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class RollingFeedbackProposalEngineChecks
{
    public static void RollingFeedbackProposalEngineKeepsResultsWithinBounds()
    {
        var engine = RollingFeedbackProposalEngine.DefaultRollingFeedbackProposalEngine(Flags.NewRegistry());
        var item = RollingFeedbackProposal.NewRollingFeedbackProposal("code-772", "label-193");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RollingFeedbackProposalEngineKeepsResultsWithinBounds();
    }
}
