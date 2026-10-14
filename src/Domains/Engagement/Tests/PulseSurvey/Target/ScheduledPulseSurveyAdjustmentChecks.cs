namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Target;

using Acme.Workforce.Engagement.PulseSurvey.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class ScheduledPulseSurveyAdjustmentChecks
{
    public static void ScheduledPulseSurveyAdjustmentStartsInPlanned()
    {
        var item = ScheduledPulseSurveyAdjustment.NewScheduledPulseSurveyAdjustment("code-955", "title-421");
        Check.Equal(item.Status, ScheduledPulseSurveyAdjustmentStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledPulseSurveyAdjustmentMovesToCompleted()
    {
        var item = ScheduledPulseSurveyAdjustment.NewScheduledPulseSurveyAdjustment("code-955", "title-421");
        try
        {
            item.MoveTo(ScheduledPulseSurveyAdjustmentStatus.Completed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledPulseSurveyAdjustmentStatus.Completed, "status");
    }

    public static void ScheduledPulseSurveyAdjustmentRejectsMovesFromCancelled()
    {
        var item = ScheduledPulseSurveyAdjustment.NewScheduledPulseSurveyAdjustment("code-955", "title-421");
        item.Status = ScheduledPulseSurveyAdjustmentStatus.Cancelled;
        Check.Fails(() => item.MoveTo(ScheduledPulseSurveyAdjustmentStatus.Planned), "move");
    }

    public static void RunAll()
    {
        ScheduledPulseSurveyAdjustmentStartsInPlanned();
        ScheduledPulseSurveyAdjustmentMovesToCompleted();
        ScheduledPulseSurveyAdjustmentRejectsMovesFromCancelled();
    }
}
