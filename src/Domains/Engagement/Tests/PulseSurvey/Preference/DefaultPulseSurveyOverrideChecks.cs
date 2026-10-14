namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Preference;

using Acme.Workforce.Engagement.PulseSurvey.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class DefaultPulseSurveyOverrideChecks
{
    public static void DefaultPulseSurveyOverrideStartsInNew()
    {
        var item = DefaultPulseSurveyOverride.NewDefaultPulseSurveyOverride("code-133", "name-793");
        Check.Equal(item.Status, DefaultPulseSurveyOverrideStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultPulseSurveyOverrideMovesToRunning()
    {
        var item = DefaultPulseSurveyOverride.NewDefaultPulseSurveyOverride("code-133", "name-793");
        try
        {
            item.MoveTo(DefaultPulseSurveyOverrideStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultPulseSurveyOverrideStatus.Running, "status");
    }

    public static void DefaultPulseSurveyOverrideRejectsMovesFromFailed()
    {
        var item = DefaultPulseSurveyOverride.NewDefaultPulseSurveyOverride("code-133", "name-793");
        item.Status = DefaultPulseSurveyOverrideStatus.Failed;
        Check.Fails(() => item.MoveTo(DefaultPulseSurveyOverrideStatus.New), "move");
    }

    public static void RunAll()
    {
        DefaultPulseSurveyOverrideStartsInNew();
        DefaultPulseSurveyOverrideMovesToRunning();
        DefaultPulseSurveyOverrideRejectsMovesFromFailed();
    }
}
