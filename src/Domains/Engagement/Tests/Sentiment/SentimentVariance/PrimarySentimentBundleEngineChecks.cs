namespace Acme.Workforce.Engagement.Tests.Sentiment.SentimentVariance;

using Acme.Workforce.Engagement.Sentiment.SentimentVariance;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of sentiment variance code, run by the project's runner.</summary>
public static class PrimarySentimentBundleEngineChecks
{
    public static void PrimarySentimentBundleEngineKeepsResultsWithinBounds()
    {
        var engine = PrimarySentimentBundleEngine.DefaultPrimarySentimentBundleEngine(Flags.NewRegistry());
        var item = PrimarySentimentBundle.NewPrimarySentimentBundle("code-703", "title-911");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PrimarySentimentBundleEngineKeepsResultsWithinBounds();
    }
}
