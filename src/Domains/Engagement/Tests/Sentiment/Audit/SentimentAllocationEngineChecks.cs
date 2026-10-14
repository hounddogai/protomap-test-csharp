namespace Acme.Workforce.Engagement.Tests.Sentiment.Audit;

using Acme.Workforce.Engagement.Sentiment.Audit;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class SentimentAllocationEngineChecks
{
    public static void SentimentAllocationEngineKeepsResultsWithinBounds()
    {
        var engine = SentimentAllocationEngine.DefaultSentimentAllocationEngine(Flags.NewRegistry());
        var item = SentimentAllocation.NewSentimentAllocation("code-746", "name-550");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SentimentAllocationEngineKeepsResultsWithinBounds();
    }
}
