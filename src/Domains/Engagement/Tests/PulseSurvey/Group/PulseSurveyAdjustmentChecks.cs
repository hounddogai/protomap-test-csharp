namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Group;

using Acme.Workforce.Engagement.PulseSurvey.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class PulseSurveyAdjustmentChecks
{
    public static void PulseSurveyAdjustmentStartsInPlanned()
    {
        var item = PulseSurveyAdjustment.NewPulseSurveyAdjustment("code-74", "name-632");
        Check.Equal(item.Status, PulseSurveyAdjustmentStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PulseSurveyAdjustmentMovesToScheduled()
    {
        var item = PulseSurveyAdjustment.NewPulseSurveyAdjustment("code-74", "name-632");
        try
        {
            item.MoveTo(PulseSurveyAdjustmentStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PulseSurveyAdjustmentStatus.Scheduled, "status");
    }

    public static void PulseSurveyAdjustmentRejectsMovesFromCancelled()
    {
        var item = PulseSurveyAdjustment.NewPulseSurveyAdjustment("code-74", "name-632");
        item.Status = PulseSurveyAdjustmentStatus.Cancelled;
        Check.Fails(() => item.MoveTo(PulseSurveyAdjustmentStatus.Planned), "move");
    }

    public static void RunAll()
    {
        PulseSurveyAdjustmentStartsInPlanned();
        PulseSurveyAdjustmentMovesToScheduled();
        PulseSurveyAdjustmentRejectsMovesFromCancelled();
    }
}
