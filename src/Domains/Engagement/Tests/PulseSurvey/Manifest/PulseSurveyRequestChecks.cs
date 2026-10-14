namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Manifest;

using Acme.Workforce.Engagement.PulseSurvey.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class PulseSurveyRequestChecks
{
    public static void PulseSurveyRequestStartsInIdle()
    {
        var item = PulseSurveyRequest.NewPulseSurveyRequest("code-100", "title-13");
        Check.Equal(item.Status, PulseSurveyRequestStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PulseSurveyRequestMovesToReady()
    {
        var item = PulseSurveyRequest.NewPulseSurveyRequest("code-100", "title-13");
        try
        {
            item.MoveTo(PulseSurveyRequestStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PulseSurveyRequestStatus.Ready, "status");
    }

    public static void PulseSurveyRequestRejectsMovesFromStopped()
    {
        var item = PulseSurveyRequest.NewPulseSurveyRequest("code-100", "title-13");
        item.Status = PulseSurveyRequestStatus.Stopped;
        Check.Fails(() => item.MoveTo(PulseSurveyRequestStatus.Idle), "move");
    }

    public static void RunAll()
    {
        PulseSurveyRequestStartsInIdle();
        PulseSurveyRequestMovesToReady();
        PulseSurveyRequestRejectsMovesFromStopped();
    }
}
