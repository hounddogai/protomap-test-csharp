namespace Acme.Workforce.Engagement.Tests.PulseSurvey.SurveyTier;

using Acme.Workforce.Engagement.PulseSurvey.SurveyTier;

/// <summary>Checks of survey tier code, run by the project's runner.</summary>
public static class PulseSurveyPlanChecks
{
    public static void PulseSurveyPlanStartsInOpen()
    {
        var item = PulseSurveyPlan.NewPulseSurveyPlan("code-913", "label-457");
        Check.Equal(item.Status, PulseSurveyPlanStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PulseSurveyPlanMovesToReopened()
    {
        var item = PulseSurveyPlan.NewPulseSurveyPlan("code-913", "label-457");
        try
        {
            item.MoveTo(PulseSurveyPlanStatus.Reopened);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PulseSurveyPlanStatus.Reopened, "status");
    }

    public static void PulseSurveyPlanRejectsMovesFromArchived()
    {
        var item = PulseSurveyPlan.NewPulseSurveyPlan("code-913", "label-457");
        item.Status = PulseSurveyPlanStatus.Archived;
        Check.Fails(() => item.MoveTo(PulseSurveyPlanStatus.Open), "move");
    }

    public static void RunAll()
    {
        PulseSurveyPlanStartsInOpen();
        PulseSurveyPlanMovesToReopened();
        PulseSurveyPlanRejectsMovesFromArchived();
    }
}
