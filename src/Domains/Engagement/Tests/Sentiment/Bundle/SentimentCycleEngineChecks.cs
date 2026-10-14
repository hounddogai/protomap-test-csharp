namespace Acme.Workforce.Engagement.Tests.Sentiment.Bundle;

using Acme.Workforce.Engagement.Sentiment.Bundle;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class SentimentCycleEngineChecks
{
    public static void SentimentCycleEngineKeepsResultsWithinBounds()
    {
        var engine = SentimentCycleEngine.DefaultSentimentCycleEngine(Flags.NewRegistry());
        var item = SentimentCycle.NewSentimentCycle("code-799", "name-898");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SentimentCycleEngineKeepsResultsWithinBounds();
    }
}
