namespace Acme.Workforce.Engagement.Tests.Sentiment.Threshold;

using Acme.Workforce.Engagement.Sentiment.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class LenientSentimentThresholdChecks
{
    public static void LenientSentimentThresholdStartsInDraft()
    {
        var item = LenientSentimentThreshold.NewLenientSentimentThreshold("code-100", "label-929");
        Check.Equal(item.Status, LenientSentimentThresholdStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientSentimentThresholdMovesToActive()
    {
        var item = LenientSentimentThreshold.NewLenientSentimentThreshold("code-100", "label-929");
        try
        {
            item.MoveTo(LenientSentimentThresholdStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientSentimentThresholdStatus.Active, "status");
    }

    public static void LenientSentimentThresholdRejectsMovesFromClosed()
    {
        var item = LenientSentimentThreshold.NewLenientSentimentThreshold("code-100", "label-929");
        item.Status = LenientSentimentThresholdStatus.Closed;
        Check.Fails(() => item.MoveTo(LenientSentimentThresholdStatus.Draft), "move");
    }

    public static void RunAll()
    {
        LenientSentimentThresholdStartsInDraft();
        LenientSentimentThresholdMovesToActive();
        LenientSentimentThresholdRejectsMovesFromClosed();
    }
}
