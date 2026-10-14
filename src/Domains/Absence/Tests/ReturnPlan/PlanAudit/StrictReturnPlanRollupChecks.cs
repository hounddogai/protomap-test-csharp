namespace Acme.Workforce.Absence.Tests.ReturnPlan.PlanAudit;

using Acme.Workforce.Absence.ReturnPlan.PlanAudit;

/// <summary>Checks of plan audit code, run by the project's runner.</summary>
public static class StrictReturnPlanRollupChecks
{
    public static void StrictReturnPlanRollupStartsInPlanned()
    {
        var item = StrictReturnPlanRollup.NewStrictReturnPlanRollup("code-87", "title-623");
        Check.Equal(item.Status, StrictReturnPlanRollupStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictReturnPlanRollupMovesToInProgress()
    {
        var item = StrictReturnPlanRollup.NewStrictReturnPlanRollup("code-87", "title-623");
        try
        {
            item.MoveTo(StrictReturnPlanRollupStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictReturnPlanRollupStatus.InProgress, "status");
    }

    public static void StrictReturnPlanRollupRejectsMovesFromCancelled()
    {
        var item = StrictReturnPlanRollup.NewStrictReturnPlanRollup("code-87", "title-623");
        item.Status = StrictReturnPlanRollupStatus.Cancelled;
        Check.Fails(() => item.MoveTo(StrictReturnPlanRollupStatus.Planned), "move");
    }

    public static void RunAll()
    {
        StrictReturnPlanRollupStartsInPlanned();
        StrictReturnPlanRollupMovesToInProgress();
        StrictReturnPlanRollupRejectsMovesFromCancelled();
    }
}
