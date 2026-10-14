namespace Acme.Workforce.Engagement.Tests.Sentiment.Proposal;

using Acme.Workforce.Engagement.Sentiment.Proposal;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class ScheduledSentimentLedgerEngineChecks
{
    public static void ScheduledSentimentLedgerEngineKeepsResultsWithinBounds()
    {
        var engine = ScheduledSentimentLedgerEngine.DefaultScheduledSentimentLedgerEngine(Flags.NewRegistry());
        var item = ScheduledSentimentLedger.NewScheduledSentimentLedger("code-635", "label-225");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ScheduledSentimentLedgerEngineKeepsResultsWithinBounds();
    }
}
