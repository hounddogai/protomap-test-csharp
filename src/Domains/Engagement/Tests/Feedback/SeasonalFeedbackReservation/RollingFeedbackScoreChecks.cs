namespace Acme.Workforce.Engagement.Tests.Feedback.SeasonalFeedbackReservation;

using Acme.Workforce.Engagement.Feedback.SeasonalFeedbackReservation;

/// <summary>Checks of seasonal feedback reservation code, run by the project's runner.</summary>
public static class RollingFeedbackScoreChecks
{
    public static void RollingFeedbackScoreStartsInOpen()
    {
        var item = RollingFeedbackScore.NewRollingFeedbackScore("code-825", "name-669");
        Check.Equal(item.Status, RollingFeedbackScoreStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RollingFeedbackScoreMovesToResolved()
    {
        var item = RollingFeedbackScore.NewRollingFeedbackScore("code-825", "name-669");
        try
        {
            item.MoveTo(RollingFeedbackScoreStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RollingFeedbackScoreStatus.Resolved, "status");
    }

    public static void RollingFeedbackScoreRejectsMovesFromArchived()
    {
        var item = RollingFeedbackScore.NewRollingFeedbackScore("code-825", "name-669");
        item.Status = RollingFeedbackScoreStatus.Archived;
        Check.Fails(() => item.MoveTo(RollingFeedbackScoreStatus.Open), "move");
    }

    public static void RunAll()
    {
        RollingFeedbackScoreStartsInOpen();
        RollingFeedbackScoreMovesToResolved();
        RollingFeedbackScoreRejectsMovesFromArchived();
    }
}
