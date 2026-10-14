namespace Acme.Workforce.Engagement.Tests.Sentiment.Audit;

using Acme.Workforce.Engagement.Sentiment.Audit;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class SentimentAuditEngineChecks
{
    public static void SentimentAuditEngineKeepsResultsWithinBounds()
    {
        var engine = SentimentAuditEngine.DefaultSentimentAuditEngine(Flags.NewRegistry());
        var item = SentimentAudit.NewSentimentAudit("code-735", "label-38");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SentimentAuditEngineKeepsResultsWithinBounds();
    }
}
