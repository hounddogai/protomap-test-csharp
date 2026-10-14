namespace Acme.Workforce.Engagement.Tests.Sentiment.Variance;

using Acme.Workforce.Engagement.Sentiment.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class PendingSentimentVarianceChecks
{
    public static void PendingSentimentVarianceStartsInNew()
    {
        var item = PendingSentimentVariance.NewPendingSentimentVariance("code-242", "name-337");
        Check.Equal(item.Status, PendingSentimentVarianceStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingSentimentVarianceMovesToDone()
    {
        var item = PendingSentimentVariance.NewPendingSentimentVariance("code-242", "name-337");
        try
        {
            item.MoveTo(PendingSentimentVarianceStatus.Done);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingSentimentVarianceStatus.Done, "status");
    }

    public static void PendingSentimentVarianceRejectsMovesFromFailed()
    {
        var item = PendingSentimentVariance.NewPendingSentimentVariance("code-242", "name-337");
        item.Status = PendingSentimentVarianceStatus.Failed;
        Check.Fails(() => item.MoveTo(PendingSentimentVarianceStatus.New), "move");
    }

    public static void RunAll()
    {
        PendingSentimentVarianceStartsInNew();
        PendingSentimentVarianceMovesToDone();
        PendingSentimentVarianceRejectsMovesFromFailed();
    }
}
