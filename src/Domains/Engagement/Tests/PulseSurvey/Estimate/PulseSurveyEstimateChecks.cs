namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Estimate;

using Acme.Workforce.Engagement.PulseSurvey.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class PulseSurveyEstimateChecks
{
    public static void PulseSurveyEstimateStartsInDraft()
    {
        var item = PulseSurveyEstimate.NewPulseSurveyEstimate("code-795", "name-705");
        Check.Equal(item.Status, PulseSurveyEstimateStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PulseSurveyEstimateMovesToPending()
    {
        var item = PulseSurveyEstimate.NewPulseSurveyEstimate("code-795", "name-705");
        try
        {
            item.MoveTo(PulseSurveyEstimateStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PulseSurveyEstimateStatus.Pending, "status");
    }

    public static void PulseSurveyEstimateRejectsMovesFromClosed()
    {
        var item = PulseSurveyEstimate.NewPulseSurveyEstimate("code-795", "name-705");
        item.Status = PulseSurveyEstimateStatus.Closed;
        Check.Fails(() => item.MoveTo(PulseSurveyEstimateStatus.Draft), "move");
    }

    public static void RunAll()
    {
        PulseSurveyEstimateStartsInDraft();
        PulseSurveyEstimateMovesToPending();
        PulseSurveyEstimateRejectsMovesFromClosed();
    }
}
