namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Variance;

using Acme.Workforce.Engagement.PulseSurvey.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class PulseSurveyPreferenceChecks
{
    public static void PulseSurveyPreferenceStartsInIdle()
    {
        var item = PulseSurveyPreference.NewPulseSurveyPreference("code-365", "label-138");
        Check.Equal(item.Status, PulseSurveyPreferenceStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PulseSurveyPreferenceMovesToDraining()
    {
        var item = PulseSurveyPreference.NewPulseSurveyPreference("code-365", "label-138");
        try
        {
            item.MoveTo(PulseSurveyPreferenceStatus.Draining);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PulseSurveyPreferenceStatus.Draining, "status");
    }

    public static void PulseSurveyPreferenceRejectsMovesFromStopped()
    {
        var item = PulseSurveyPreference.NewPulseSurveyPreference("code-365", "label-138");
        item.Status = PulseSurveyPreferenceStatus.Stopped;
        Check.Fails(() => item.MoveTo(PulseSurveyPreferenceStatus.Idle), "move");
    }

    public static void RunAll()
    {
        PulseSurveyPreferenceStartsInIdle();
        PulseSurveyPreferenceMovesToDraining();
        PulseSurveyPreferenceRejectsMovesFromStopped();
    }
}
