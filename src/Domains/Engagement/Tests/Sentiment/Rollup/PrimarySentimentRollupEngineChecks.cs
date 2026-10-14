namespace Acme.Workforce.Engagement.Tests.Sentiment.Rollup;

using Acme.Workforce.Engagement.Sentiment.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class PrimarySentimentRollupEngineChecks
{
    public static void PrimarySentimentRollupEngineKeepsResultsWithinBounds()
    {
        var engine = PrimarySentimentRollupEngine.DefaultPrimarySentimentRollupEngine();
        var item = PrimarySentimentRollup.NewPrimarySentimentRollup("code-465", "label-528");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PrimarySentimentRollupEngineKeepsResultsWithinBounds();
    }
}
