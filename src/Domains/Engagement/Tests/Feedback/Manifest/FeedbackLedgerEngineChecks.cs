namespace Acme.Workforce.Engagement.Tests.Feedback.Manifest;

using Acme.Workforce.Engagement.Feedback.Manifest;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class FeedbackLedgerEngineChecks
{
    public static void FeedbackLedgerEngineKeepsResultsWithinBounds()
    {
        var engine = FeedbackLedgerEngine.DefaultFeedbackLedgerEngine(Flags.NewRegistry());
        var item = FeedbackLedger.NewFeedbackLedger("code-560", "label-163");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        FeedbackLedgerEngineKeepsResultsWithinBounds();
    }
}
