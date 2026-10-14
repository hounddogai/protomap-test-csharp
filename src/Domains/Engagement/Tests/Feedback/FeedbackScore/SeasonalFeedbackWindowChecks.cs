namespace Acme.Workforce.Engagement.Tests.Feedback.FeedbackScore;

using Acme.Workforce.Engagement.Feedback.FeedbackScore;

/// <summary>Checks of feedback score code, run by the project's runner.</summary>
public static class SeasonalFeedbackWindowChecks
{
    public static void SeasonalFeedbackWindowStartsInNew()
    {
        var item = SeasonalFeedbackWindow.NewSeasonalFeedbackWindow("code-426", "name-731");
        Check.Equal(item.Status, SeasonalFeedbackWindowStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalFeedbackWindowMovesToFailed()
    {
        var item = SeasonalFeedbackWindow.NewSeasonalFeedbackWindow("code-426", "name-731");
        try
        {
            item.MoveTo(SeasonalFeedbackWindowStatus.Failed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalFeedbackWindowStatus.Failed, "status");
    }

    public static void SeasonalFeedbackWindowRejectsMovesFromFailed()
    {
        var item = SeasonalFeedbackWindow.NewSeasonalFeedbackWindow("code-426", "name-731");
        item.Status = SeasonalFeedbackWindowStatus.Failed;
        Check.Fails(() => item.MoveTo(SeasonalFeedbackWindowStatus.New), "move");
    }

    public static void RunAll()
    {
        SeasonalFeedbackWindowStartsInNew();
        SeasonalFeedbackWindowMovesToFailed();
        SeasonalFeedbackWindowRejectsMovesFromFailed();
    }
}
