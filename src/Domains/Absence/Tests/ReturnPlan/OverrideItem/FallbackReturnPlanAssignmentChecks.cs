namespace Acme.Workforce.Absence.Tests.ReturnPlan.OverrideItem;

using Acme.Workforce.Absence.ReturnPlan.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class FallbackReturnPlanAssignmentChecks
{
    public static void FallbackReturnPlanAssignmentStartsInPlanned()
    {
        var item = FallbackReturnPlanAssignment.NewFallbackReturnPlanAssignment("code-885", "label-835");
        Check.Equal(item.Status, FallbackReturnPlanAssignmentStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackReturnPlanAssignmentMovesToScheduled()
    {
        var item = FallbackReturnPlanAssignment.NewFallbackReturnPlanAssignment("code-885", "label-835");
        try
        {
            item.MoveTo(FallbackReturnPlanAssignmentStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackReturnPlanAssignmentStatus.Scheduled, "status");
    }

    public static void FallbackReturnPlanAssignmentRejectsMovesFromCancelled()
    {
        var item = FallbackReturnPlanAssignment.NewFallbackReturnPlanAssignment("code-885", "label-835");
        item.Status = FallbackReturnPlanAssignmentStatus.Cancelled;
        Check.Fails(() => item.MoveTo(FallbackReturnPlanAssignmentStatus.Planned), "move");
    }

    public static void RunAll()
    {
        FallbackReturnPlanAssignmentStartsInPlanned();
        FallbackReturnPlanAssignmentMovesToScheduled();
        FallbackReturnPlanAssignmentRejectsMovesFromCancelled();
    }
}
