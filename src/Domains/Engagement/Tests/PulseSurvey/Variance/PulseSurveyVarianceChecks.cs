namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Variance;

using Acme.Workforce.Engagement.PulseSurvey.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class PulseSurveyVarianceChecks
{
    public static void PulseSurveyVarianceStartsInDraft()
    {
        var item = PulseSurveyVariance.NewPulseSurveyVariance("code-531", "name-194");
        Check.Equal(item.Status, PulseSurveyVarianceStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PulseSurveyVarianceMovesToPending()
    {
        var item = PulseSurveyVariance.NewPulseSurveyVariance("code-531", "name-194");
        try
        {
            item.MoveTo(PulseSurveyVarianceStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PulseSurveyVarianceStatus.Pending, "status");
    }

    public static void PulseSurveyVarianceRejectsMovesFromClosed()
    {
        var item = PulseSurveyVariance.NewPulseSurveyVariance("code-531", "name-194");
        item.Status = PulseSurveyVarianceStatus.Closed;
        Check.Fails(() => item.MoveTo(PulseSurveyVarianceStatus.Draft), "move");
    }

    public static void RunAll()
    {
        PulseSurveyVarianceStartsInDraft();
        PulseSurveyVarianceMovesToPending();
        PulseSurveyVarianceRejectsMovesFromClosed();
    }
}
