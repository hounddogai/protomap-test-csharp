namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Assignment;

using Acme.Workforce.Engagement.PulseSurvey.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class PulseSurveyScoreChecks
{
    public static void PulseSurveyScoreStartsInIdle()
    {
        var item = PulseSurveyScore.NewPulseSurveyScore("code-514", "name-829");
        Check.Equal(item.Status, PulseSurveyScoreStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PulseSurveyScoreMovesToReady()
    {
        var item = PulseSurveyScore.NewPulseSurveyScore("code-514", "name-829");
        try
        {
            item.MoveTo(PulseSurveyScoreStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PulseSurveyScoreStatus.Ready, "status");
    }

    public static void PulseSurveyScoreRejectsMovesFromStopped()
    {
        var item = PulseSurveyScore.NewPulseSurveyScore("code-514", "name-829");
        item.Status = PulseSurveyScoreStatus.Stopped;
        Check.Fails(() => item.MoveTo(PulseSurveyScoreStatus.Idle), "move");
    }

    public static void RunAll()
    {
        PulseSurveyScoreStartsInIdle();
        PulseSurveyScoreMovesToReady();
        PulseSurveyScoreRejectsMovesFromStopped();
    }
}
