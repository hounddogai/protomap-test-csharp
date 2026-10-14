namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Assignment;

using Acme.Workforce.Engagement.PulseSurvey.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class PulseSurveyRecordChecks
{
    public static void PulseSurveyRecordStartsInIdle()
    {
        var item = PulseSurveyRecord.NewPulseSurveyRecord("code-936", "name-423");
        Check.Equal(item.Status, PulseSurveyRecordStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PulseSurveyRecordMovesToStopped()
    {
        var item = PulseSurveyRecord.NewPulseSurveyRecord("code-936", "name-423");
        try
        {
            item.MoveTo(PulseSurveyRecordStatus.Stopped);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PulseSurveyRecordStatus.Stopped, "status");
    }

    public static void PulseSurveyRecordRejectsMovesFromStopped()
    {
        var item = PulseSurveyRecord.NewPulseSurveyRecord("code-936", "name-423");
        item.Status = PulseSurveyRecordStatus.Stopped;
        Check.Fails(() => item.MoveTo(PulseSurveyRecordStatus.Idle), "move");
    }

    public static void RunAll()
    {
        PulseSurveyRecordStartsInIdle();
        PulseSurveyRecordMovesToStopped();
        PulseSurveyRecordRejectsMovesFromStopped();
    }
}
