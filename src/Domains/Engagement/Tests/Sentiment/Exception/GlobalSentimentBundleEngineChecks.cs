namespace Acme.Workforce.Engagement.Tests.Sentiment.Exception;

using Acme.Workforce.Engagement.Sentiment.Exception;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class GlobalSentimentBundleEngineChecks
{
    public static void GlobalSentimentBundleEngineKeepsResultsWithinBounds()
    {
        var engine = GlobalSentimentBundleEngine.DefaultGlobalSentimentBundleEngine(Flags.NewRegistry());
        var item = GlobalSentimentBundle.NewGlobalSentimentBundle("code-374", "name-222");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        GlobalSentimentBundleEngineKeepsResultsWithinBounds();
    }
}
