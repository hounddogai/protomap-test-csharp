namespace Acme.Workforce.Engagement.Tests.PulseSurvey.SurveyAudit;

using Acme.Workforce.Engagement.PulseSurvey.SurveyAudit;

/// <summary>Checks of survey audit code, run by the project's runner.</summary>
public static class GlobalPulseSurveyAdjustmentChecks
{
    public static void GlobalPulseSurveyAdjustmentStartsInOpen()
    {
        var item = GlobalPulseSurveyAdjustment.NewGlobalPulseSurveyAdjustment("code-881", "label-289");
        Check.Equal(item.Status, GlobalPulseSurveyAdjustmentStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GlobalPulseSurveyAdjustmentMovesToReopened()
    {
        var item = GlobalPulseSurveyAdjustment.NewGlobalPulseSurveyAdjustment("code-881", "label-289");
        try
        {
            item.MoveTo(GlobalPulseSurveyAdjustmentStatus.Reopened);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GlobalPulseSurveyAdjustmentStatus.Reopened, "status");
    }

    public static void GlobalPulseSurveyAdjustmentRejectsMovesFromArchived()
    {
        var item = GlobalPulseSurveyAdjustment.NewGlobalPulseSurveyAdjustment("code-881", "label-289");
        item.Status = GlobalPulseSurveyAdjustmentStatus.Archived;
        Check.Fails(() => item.MoveTo(GlobalPulseSurveyAdjustmentStatus.Open), "move");
    }

    public static void RunAll()
    {
        GlobalPulseSurveyAdjustmentStartsInOpen();
        GlobalPulseSurveyAdjustmentMovesToReopened();
        GlobalPulseSurveyAdjustmentRejectsMovesFromArchived();
    }
}
