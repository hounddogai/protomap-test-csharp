namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Exception;

using Acme.Workforce.Engagement.PulseSurvey.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class PulseSurveyTierChecks
{
    public static void PulseSurveyTierStartsInRequested()
    {
        var item = PulseSurveyTier.NewPulseSurveyTier("code-136", "name-363");
        Check.Equal(item.Status, PulseSurveyTierStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PulseSurveyTierMovesToGranted()
    {
        var item = PulseSurveyTier.NewPulseSurveyTier("code-136", "name-363");
        try
        {
            item.MoveTo(PulseSurveyTierStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PulseSurveyTierStatus.Granted, "status");
    }

    public static void PulseSurveyTierRejectsMovesFromExpired()
    {
        var item = PulseSurveyTier.NewPulseSurveyTier("code-136", "name-363");
        item.Status = PulseSurveyTierStatus.Expired;
        Check.Fails(() => item.MoveTo(PulseSurveyTierStatus.Requested), "move");
    }

    public static void RunAll()
    {
        PulseSurveyTierStartsInRequested();
        PulseSurveyTierMovesToGranted();
        PulseSurveyTierRejectsMovesFromExpired();
    }
}
