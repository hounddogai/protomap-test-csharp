namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Preference;

using Acme.Workforce.Engagement.PulseSurvey.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class StrictPulseSurveyPreferenceChecks
{
    public static void StrictPulseSurveyPreferenceStartsInNew()
    {
        var item = StrictPulseSurveyPreference.NewStrictPulseSurveyPreference("code-334", "name-888");
        Check.Equal(item.Status, StrictPulseSurveyPreferenceStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictPulseSurveyPreferenceMovesToRunning()
    {
        var item = StrictPulseSurveyPreference.NewStrictPulseSurveyPreference("code-334", "name-888");
        try
        {
            item.MoveTo(StrictPulseSurveyPreferenceStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictPulseSurveyPreferenceStatus.Running, "status");
    }

    public static void StrictPulseSurveyPreferenceRejectsMovesFromFailed()
    {
        var item = StrictPulseSurveyPreference.NewStrictPulseSurveyPreference("code-334", "name-888");
        item.Status = StrictPulseSurveyPreferenceStatus.Failed;
        Check.Fails(() => item.MoveTo(StrictPulseSurveyPreferenceStatus.New), "move");
    }

    public static void RunAll()
    {
        StrictPulseSurveyPreferenceStartsInNew();
        StrictPulseSurveyPreferenceMovesToRunning();
        StrictPulseSurveyPreferenceRejectsMovesFromFailed();
    }
}
