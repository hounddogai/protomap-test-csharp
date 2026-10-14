namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Estimate;

using Acme.Workforce.Engagement.PulseSurvey.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class PulseSurveyScheduleChecks
{
    public static void PulseSurveyScheduleStartsInNew()
    {
        var item = PulseSurveySchedule.NewPulseSurveySchedule("code-440", "title-558");
        Check.Equal(item.Status, PulseSurveyScheduleStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PulseSurveyScheduleMovesToQueued()
    {
        var item = PulseSurveySchedule.NewPulseSurveySchedule("code-440", "title-558");
        try
        {
            item.MoveTo(PulseSurveyScheduleStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PulseSurveyScheduleStatus.Queued, "status");
    }

    public static void PulseSurveyScheduleRejectsMovesFromFailed()
    {
        var item = PulseSurveySchedule.NewPulseSurveySchedule("code-440", "title-558");
        item.Status = PulseSurveyScheduleStatus.Failed;
        Check.Fails(() => item.MoveTo(PulseSurveyScheduleStatus.New), "move");
    }

    public static void RunAll()
    {
        PulseSurveyScheduleStartsInNew();
        PulseSurveyScheduleMovesToQueued();
        PulseSurveyScheduleRejectsMovesFromFailed();
    }
}
