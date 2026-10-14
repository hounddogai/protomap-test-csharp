namespace Acme.Workforce.Engagement.Tests.Sentiment.Threshold;

using Acme.Workforce.Engagement.Sentiment.Threshold;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class LenientSentimentThresholdEngineChecks
{
    public static void LenientSentimentThresholdEngineKeepsResultsWithinBounds()
    {
        var engine = LenientSentimentThresholdEngine.DefaultLenientSentimentThresholdEngine(Flags.NewRegistry());
        var item = LenientSentimentThreshold.NewLenientSentimentThreshold("code-100", "label-929");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LenientSentimentThresholdEngineKeepsResultsWithinBounds();
    }
}
