namespace Acme.Workforce.Engagement.Tests.Sentiment.Manifest;

using Acme.Workforce.Engagement.Sentiment.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class LenientSentimentEstimateChecks
{
    public static void LenientSentimentEstimateStartsInDraft()
    {
        var item = LenientSentimentEstimate.NewLenientSentimentEstimate("code-357", "title-91");
        Check.Equal(item.Status, LenientSentimentEstimateStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientSentimentEstimateMovesToClosed()
    {
        var item = LenientSentimentEstimate.NewLenientSentimentEstimate("code-357", "title-91");
        try
        {
            item.MoveTo(LenientSentimentEstimateStatus.Closed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientSentimentEstimateStatus.Closed, "status");
    }

    public static void LenientSentimentEstimateRejectsMovesFromClosed()
    {
        var item = LenientSentimentEstimate.NewLenientSentimentEstimate("code-357", "title-91");
        item.Status = LenientSentimentEstimateStatus.Closed;
        Check.Fails(() => item.MoveTo(LenientSentimentEstimateStatus.Draft), "move");
    }

    public static void RunAll()
    {
        LenientSentimentEstimateStartsInDraft();
        LenientSentimentEstimateMovesToClosed();
        LenientSentimentEstimateRejectsMovesFromClosed();
    }
}
