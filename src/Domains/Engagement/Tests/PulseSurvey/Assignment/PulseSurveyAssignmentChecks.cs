namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Assignment;

using Acme.Workforce.Engagement.PulseSurvey.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class PulseSurveyAssignmentChecks
{
    public static void PulseSurveyAssignmentStartsInIdle()
    {
        var item = PulseSurveyAssignment.NewPulseSurveyAssignment("code-591", "name-355");
        Check.Equal(item.Status, PulseSurveyAssignmentStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PulseSurveyAssignmentMovesToReady()
    {
        var item = PulseSurveyAssignment.NewPulseSurveyAssignment("code-591", "name-355");
        try
        {
            item.MoveTo(PulseSurveyAssignmentStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PulseSurveyAssignmentStatus.Ready, "status");
    }

    public static void PulseSurveyAssignmentRejectsMovesFromStopped()
    {
        var item = PulseSurveyAssignment.NewPulseSurveyAssignment("code-591", "name-355");
        item.Status = PulseSurveyAssignmentStatus.Stopped;
        Check.Fails(() => item.MoveTo(PulseSurveyAssignmentStatus.Idle), "move");
    }

    public static void RunAll()
    {
        PulseSurveyAssignmentStartsInIdle();
        PulseSurveyAssignmentMovesToReady();
        PulseSurveyAssignmentRejectsMovesFromStopped();
    }
}
