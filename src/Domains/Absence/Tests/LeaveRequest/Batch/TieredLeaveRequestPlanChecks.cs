namespace Acme.Workforce.Absence.Tests.LeaveRequest.Batch;

using Acme.Workforce.Absence.LeaveRequest.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class TieredLeaveRequestPlanChecks
{
    public static void TieredLeaveRequestPlanStartsInNew()
    {
        var item = TieredLeaveRequestPlan.NewTieredLeaveRequestPlan("code-105", "label-718");
        Check.Equal(item.Status, TieredLeaveRequestPlanStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredLeaveRequestPlanMovesToDone()
    {
        var item = TieredLeaveRequestPlan.NewTieredLeaveRequestPlan("code-105", "label-718");
        try
        {
            item.MoveTo(TieredLeaveRequestPlanStatus.Done);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredLeaveRequestPlanStatus.Done, "status");
    }

    public static void TieredLeaveRequestPlanRejectsMovesFromFailed()
    {
        var item = TieredLeaveRequestPlan.NewTieredLeaveRequestPlan("code-105", "label-718");
        item.Status = TieredLeaveRequestPlanStatus.Failed;
        Check.Fails(() => item.MoveTo(TieredLeaveRequestPlanStatus.New), "move");
    }

    public static void RunAll()
    {
        TieredLeaveRequestPlanStartsInNew();
        TieredLeaveRequestPlanMovesToDone();
        TieredLeaveRequestPlanRejectsMovesFromFailed();
    }
}
