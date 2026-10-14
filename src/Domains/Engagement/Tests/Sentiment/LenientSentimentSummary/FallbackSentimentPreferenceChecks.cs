namespace Acme.Workforce.Engagement.Tests.Sentiment.LenientSentimentSummary;

using Acme.Workforce.Engagement.Sentiment.LenientSentimentSummary;

/// <summary>Checks of lenient sentiment summary code, run by the project's runner.</summary>
public static class FallbackSentimentPreferenceChecks
{
    public static void FallbackSentimentPreferenceStartsInIdle()
    {
        var item = FallbackSentimentPreference.NewFallbackSentimentPreference("code-704", "title-412");
        Check.Equal(item.Status, FallbackSentimentPreferenceStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackSentimentPreferenceMovesToReady()
    {
        var item = FallbackSentimentPreference.NewFallbackSentimentPreference("code-704", "title-412");
        try
        {
            item.MoveTo(FallbackSentimentPreferenceStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackSentimentPreferenceStatus.Ready, "status");
    }

    public static void FallbackSentimentPreferenceRejectsMovesFromStopped()
    {
        var item = FallbackSentimentPreference.NewFallbackSentimentPreference("code-704", "title-412");
        item.Status = FallbackSentimentPreferenceStatus.Stopped;
        Check.Fails(() => item.MoveTo(FallbackSentimentPreferenceStatus.Idle), "move");
    }

    public static void RunAll()
    {
        FallbackSentimentPreferenceStartsInIdle();
        FallbackSentimentPreferenceMovesToReady();
        FallbackSentimentPreferenceRejectsMovesFromStopped();
    }
}
