namespace Acme.Workforce.Engagement.Tests.Sentiment.Rollup;

using Acme.Workforce.Engagement.Sentiment.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class LegacySentimentRuleEngineChecks
{
    public static void LegacySentimentRuleEngineKeepsResultsWithinBounds()
    {
        var engine = LegacySentimentRuleEngine.DefaultLegacySentimentRuleEngine();
        var item = LegacySentimentRule.NewLegacySentimentRule("code-413", "title-485");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LegacySentimentRuleEngineKeepsResultsWithinBounds();
    }
}
