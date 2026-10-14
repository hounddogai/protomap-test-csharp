namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Manifest;

using Acme.Workforce.Engagement.PulseSurvey.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class StrictPulseSurveyVarianceChecks
{
    public static void StrictPulseSurveyVarianceStartsInOpen()
    {
        var item = StrictPulseSurveyVariance.NewStrictPulseSurveyVariance("code-104", "title-24");
        Check.Equal(item.Status, StrictPulseSurveyVarianceStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictPulseSurveyVarianceMovesToReopened()
    {
        var item = StrictPulseSurveyVariance.NewStrictPulseSurveyVariance("code-104", "title-24");
        try
        {
            item.MoveTo(StrictPulseSurveyVarianceStatus.Reopened);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictPulseSurveyVarianceStatus.Reopened, "status");
    }

    public static void StrictPulseSurveyVarianceRejectsMovesFromArchived()
    {
        var item = StrictPulseSurveyVariance.NewStrictPulseSurveyVariance("code-104", "title-24");
        item.Status = StrictPulseSurveyVarianceStatus.Archived;
        Check.Fails(() => item.MoveTo(StrictPulseSurveyVarianceStatus.Open), "move");
    }

    public static void RunAll()
    {
        StrictPulseSurveyVarianceStartsInOpen();
        StrictPulseSurveyVarianceMovesToReopened();
        StrictPulseSurveyVarianceRejectsMovesFromArchived();
    }
}
