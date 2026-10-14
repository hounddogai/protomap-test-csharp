namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Template;

using Acme.Workforce.Engagement.PulseSurvey.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class PulseSurveyRollupChecks
{
    public static void PulseSurveyRollupStartsInRequested()
    {
        var item = PulseSurveyRollup.NewPulseSurveyRollup("code-84", "title-537");
        Check.Equal(item.Status, PulseSurveyRollupStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PulseSurveyRollupMovesToGranted()
    {
        var item = PulseSurveyRollup.NewPulseSurveyRollup("code-84", "title-537");
        try
        {
            item.MoveTo(PulseSurveyRollupStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PulseSurveyRollupStatus.Granted, "status");
    }

    public static void PulseSurveyRollupRejectsMovesFromExpired()
    {
        var item = PulseSurveyRollup.NewPulseSurveyRollup("code-84", "title-537");
        item.Status = PulseSurveyRollupStatus.Expired;
        Check.Fails(() => item.MoveTo(PulseSurveyRollupStatus.Requested), "move");
    }

    public static void RunAll()
    {
        PulseSurveyRollupStartsInRequested();
        PulseSurveyRollupMovesToGranted();
        PulseSurveyRollupRejectsMovesFromExpired();
    }
}
