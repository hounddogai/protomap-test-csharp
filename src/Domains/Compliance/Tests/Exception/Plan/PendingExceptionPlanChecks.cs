namespace Acme.Workforce.Compliance.Tests.Exception.Plan;

using Acme.Workforce.Compliance.Exception.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class PendingExceptionPlanChecks
{
    public static void PendingExceptionPlanStartsInPlanned()
    {
        var item = PendingExceptionPlan.NewPendingExceptionPlan("code-812", "title-271");
        Check.Equal(item.Status, PendingExceptionPlanStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingExceptionPlanMovesToScheduled()
    {
        var item = PendingExceptionPlan.NewPendingExceptionPlan("code-812", "title-271");
        try
        {
            item.MoveTo(PendingExceptionPlanStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingExceptionPlanStatus.Scheduled, "status");
    }

    public static void PendingExceptionPlanRejectsMovesFromCancelled()
    {
        var item = PendingExceptionPlan.NewPendingExceptionPlan("code-812", "title-271");
        item.Status = PendingExceptionPlanStatus.Cancelled;
        Check.Fails(() => item.MoveTo(PendingExceptionPlanStatus.Planned), "move");
    }

    public static void RunAll()
    {
        PendingExceptionPlanStartsInPlanned();
        PendingExceptionPlanMovesToScheduled();
        PendingExceptionPlanRejectsMovesFromCancelled();
    }
}
