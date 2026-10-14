namespace Acme.Workforce.Engagement.Tests.Feedback.FeedbackScore;

using Acme.Workforce.Engagement.Feedback.FeedbackScore;

/// <summary>Checks of feedback score code, run by the project's runner.</summary>
public static class SeasonalFeedbackScoreChecks
{
    public static void SeasonalFeedbackScoreStartsInIdle()
    {
        var item = SeasonalFeedbackScore.NewSeasonalFeedbackScore("code-214", "title-530");
        Check.Equal(item.Status, SeasonalFeedbackScoreStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalFeedbackScoreMovesToReady()
    {
        var item = SeasonalFeedbackScore.NewSeasonalFeedbackScore("code-214", "title-530");
        try
        {
            item.MoveTo(SeasonalFeedbackScoreStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalFeedbackScoreStatus.Ready, "status");
    }

    public static void SeasonalFeedbackScoreRejectsMovesFromStopped()
    {
        var item = SeasonalFeedbackScore.NewSeasonalFeedbackScore("code-214", "title-530");
        item.Status = SeasonalFeedbackScoreStatus.Stopped;
        Check.Fails(() => item.MoveTo(SeasonalFeedbackScoreStatus.Idle), "move");
    }

    public static void RunAll()
    {
        SeasonalFeedbackScoreStartsInIdle();
        SeasonalFeedbackScoreMovesToReady();
        SeasonalFeedbackScoreRejectsMovesFromStopped();
    }
}
