namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Exception;

using Acme.Workforce.Engagement.PulseSurvey.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class PulseSurveyExceptionChecks
{
    public static void PulseSurveyExceptionStartsInRequested()
    {
        var item = PulseSurveyException.NewPulseSurveyException("code-797", "title-704");
        Check.Equal(item.Status, PulseSurveyExceptionStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PulseSurveyExceptionMovesToGranted()
    {
        var item = PulseSurveyException.NewPulseSurveyException("code-797", "title-704");
        try
        {
            item.MoveTo(PulseSurveyExceptionStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PulseSurveyExceptionStatus.Granted, "status");
    }

    public static void PulseSurveyExceptionRejectsMovesFromExpired()
    {
        var item = PulseSurveyException.NewPulseSurveyException("code-797", "title-704");
        item.Status = PulseSurveyExceptionStatus.Expired;
        Check.Fails(() => item.MoveTo(PulseSurveyExceptionStatus.Requested), "move");
    }

    public static void RunAll()
    {
        PulseSurveyExceptionStartsInRequested();
        PulseSurveyExceptionMovesToGranted();
        PulseSurveyExceptionRejectsMovesFromExpired();
    }
}
