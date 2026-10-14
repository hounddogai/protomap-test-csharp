namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Variance;

using Acme.Workforce.Engagement.PulseSurvey.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class PulseSurveyOverrideChecks
{
    public static void PulseSurveyOverrideStartsInNew()
    {
        var item = PulseSurveyOverride.NewPulseSurveyOverride("code-232", "name-981");
        Check.Equal(item.Status, PulseSurveyOverrideStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PulseSurveyOverrideMovesToDone()
    {
        var item = PulseSurveyOverride.NewPulseSurveyOverride("code-232", "name-981");
        try
        {
            item.MoveTo(PulseSurveyOverrideStatus.Done);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PulseSurveyOverrideStatus.Done, "status");
    }

    public static void PulseSurveyOverrideRejectsMovesFromFailed()
    {
        var item = PulseSurveyOverride.NewPulseSurveyOverride("code-232", "name-981");
        item.Status = PulseSurveyOverrideStatus.Failed;
        Check.Fails(() => item.MoveTo(PulseSurveyOverrideStatus.New), "move");
    }

    public static void RunAll()
    {
        PulseSurveyOverrideStartsInNew();
        PulseSurveyOverrideMovesToDone();
        PulseSurveyOverrideRejectsMovesFromFailed();
    }
}
