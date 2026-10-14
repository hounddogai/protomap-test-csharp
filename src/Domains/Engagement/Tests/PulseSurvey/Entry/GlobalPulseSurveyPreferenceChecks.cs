namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Entry;

using Acme.Workforce.Engagement.PulseSurvey.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class GlobalPulseSurveyPreferenceChecks
{
    public static void GlobalPulseSurveyPreferenceStartsInRequested()
    {
        var item = GlobalPulseSurveyPreference.NewGlobalPulseSurveyPreference("code-778", "title-778");
        Check.Equal(item.Status, GlobalPulseSurveyPreferenceStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GlobalPulseSurveyPreferenceMovesToGranted()
    {
        var item = GlobalPulseSurveyPreference.NewGlobalPulseSurveyPreference("code-778", "title-778");
        try
        {
            item.MoveTo(GlobalPulseSurveyPreferenceStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GlobalPulseSurveyPreferenceStatus.Granted, "status");
    }

    public static void GlobalPulseSurveyPreferenceRejectsMovesFromExpired()
    {
        var item = GlobalPulseSurveyPreference.NewGlobalPulseSurveyPreference("code-778", "title-778");
        item.Status = GlobalPulseSurveyPreferenceStatus.Expired;
        Check.Fails(() => item.MoveTo(GlobalPulseSurveyPreferenceStatus.Requested), "move");
    }

    public static void RunAll()
    {
        GlobalPulseSurveyPreferenceStartsInRequested();
        GlobalPulseSurveyPreferenceMovesToGranted();
        GlobalPulseSurveyPreferenceRejectsMovesFromExpired();
    }
}
