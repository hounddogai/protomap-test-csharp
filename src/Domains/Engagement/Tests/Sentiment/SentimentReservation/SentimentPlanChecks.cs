namespace Acme.Workforce.Engagement.Tests.Sentiment.SentimentReservation;

using Acme.Workforce.Engagement.Sentiment.SentimentReservation;

/// <summary>Checks of sentiment reservation code, run by the project's runner.</summary>
public static class SentimentPlanChecks
{
    public static void SentimentPlanStartsInIdle()
    {
        var item = SentimentPlan.NewSentimentPlan("code-375", "label-994");
        Check.Equal(item.Status, SentimentPlanStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SentimentPlanMovesToWarming()
    {
        var item = SentimentPlan.NewSentimentPlan("code-375", "label-994");
        try
        {
            item.MoveTo(SentimentPlanStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SentimentPlanStatus.Warming, "status");
    }

    public static void SentimentPlanRejectsMovesFromStopped()
    {
        var item = SentimentPlan.NewSentimentPlan("code-375", "label-994");
        item.Status = SentimentPlanStatus.Stopped;
        Check.Fails(() => item.MoveTo(SentimentPlanStatus.Idle), "move");
    }

    public static void RunAll()
    {
        SentimentPlanStartsInIdle();
        SentimentPlanMovesToWarming();
        SentimentPlanRejectsMovesFromStopped();
    }
}
