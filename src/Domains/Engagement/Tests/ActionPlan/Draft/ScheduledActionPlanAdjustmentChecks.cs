namespace Acme.Workforce.Engagement.Tests.ActionPlan.Draft;

using Acme.Workforce.Engagement.ActionPlan.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class ScheduledActionPlanAdjustmentChecks
{
    public static void ScheduledActionPlanAdjustmentStartsInPlanned()
    {
        var item = ScheduledActionPlanAdjustment.NewScheduledActionPlanAdjustment("code-485", "name-746");
        Check.Equal(item.Status, ScheduledActionPlanAdjustmentStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledActionPlanAdjustmentMovesToScheduled()
    {
        var item = ScheduledActionPlanAdjustment.NewScheduledActionPlanAdjustment("code-485", "name-746");
        try
        {
            item.MoveTo(ScheduledActionPlanAdjustmentStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledActionPlanAdjustmentStatus.Scheduled, "status");
    }

    public static void ScheduledActionPlanAdjustmentRejectsMovesFromCancelled()
    {
        var item = ScheduledActionPlanAdjustment.NewScheduledActionPlanAdjustment("code-485", "name-746");
        item.Status = ScheduledActionPlanAdjustmentStatus.Cancelled;
        Check.Fails(() => item.MoveTo(ScheduledActionPlanAdjustmentStatus.Planned), "move");
    }

    public static void RunAll()
    {
        ScheduledActionPlanAdjustmentStartsInPlanned();
        ScheduledActionPlanAdjustmentMovesToScheduled();
        ScheduledActionPlanAdjustmentRejectsMovesFromCancelled();
    }
}
