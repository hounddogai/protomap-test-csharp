namespace Acme.Workforce.Workplace.Tests.FloorPlan.Milestone;

using Acme.Workforce.Workplace.FloorPlan.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class PendingFloorPlanTargetChecks
{
    public static void PendingFloorPlanTargetStartsInProposed()
    {
        var item = PendingFloorPlanTarget.NewPendingFloorPlanTarget("code-427", "label-236");
        Check.Equal(item.Status, PendingFloorPlanTargetStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingFloorPlanTargetMovesToRejected()
    {
        var item = PendingFloorPlanTarget.NewPendingFloorPlanTarget("code-427", "label-236");
        try
        {
            item.MoveTo(PendingFloorPlanTargetStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingFloorPlanTargetStatus.Rejected, "status");
    }

    public static void PendingFloorPlanTargetRejectsMovesFromWithdrawn()
    {
        var item = PendingFloorPlanTarget.NewPendingFloorPlanTarget("code-427", "label-236");
        item.Status = PendingFloorPlanTargetStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(PendingFloorPlanTargetStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        PendingFloorPlanTargetStartsInProposed();
        PendingFloorPlanTargetMovesToRejected();
        PendingFloorPlanTargetRejectsMovesFromWithdrawn();
    }
}
