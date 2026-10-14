namespace Acme.Workforce.Engagement.Tests.Sentiment.SentimentVariance;

using Acme.Workforce.Engagement.Sentiment.SentimentVariance;

/// <summary>Checks of sentiment variance code, run by the project's runner.</summary>
public static class PrimarySentimentBundleChecks
{
    public static void PrimarySentimentBundleStartsInPlanned()
    {
        var item = PrimarySentimentBundle.NewPrimarySentimentBundle("code-703", "title-911");
        Check.Equal(item.Status, PrimarySentimentBundleStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PrimarySentimentBundleMovesToInProgress()
    {
        var item = PrimarySentimentBundle.NewPrimarySentimentBundle("code-703", "title-911");
        try
        {
            item.MoveTo(PrimarySentimentBundleStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PrimarySentimentBundleStatus.InProgress, "status");
    }

    public static void PrimarySentimentBundleRejectsMovesFromCancelled()
    {
        var item = PrimarySentimentBundle.NewPrimarySentimentBundle("code-703", "title-911");
        item.Status = PrimarySentimentBundleStatus.Cancelled;
        Check.Fails(() => item.MoveTo(PrimarySentimentBundleStatus.Planned), "move");
    }

    public static void RunAll()
    {
        PrimarySentimentBundleStartsInPlanned();
        PrimarySentimentBundleMovesToInProgress();
        PrimarySentimentBundleRejectsMovesFromCancelled();
    }
}
