namespace Acme.Workforce.Workplace.Tests.FloorPlan.Reservation;

using Acme.Workforce.Workplace.FloorPlan.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class CappedFloorPlanThresholdChecks
{
    public static void CappedFloorPlanThresholdStartsInProposed()
    {
        var item = CappedFloorPlanThreshold.NewCappedFloorPlanThreshold("code-898", "label-846");
        Check.Equal(item.Status, CappedFloorPlanThresholdStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedFloorPlanThresholdMovesToApproved()
    {
        var item = CappedFloorPlanThreshold.NewCappedFloorPlanThreshold("code-898", "label-846");
        try
        {
            item.MoveTo(CappedFloorPlanThresholdStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedFloorPlanThresholdStatus.Approved, "status");
    }

    public static void CappedFloorPlanThresholdRejectsMovesFromWithdrawn()
    {
        var item = CappedFloorPlanThreshold.NewCappedFloorPlanThreshold("code-898", "label-846");
        item.Status = CappedFloorPlanThresholdStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(CappedFloorPlanThresholdStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        CappedFloorPlanThresholdStartsInProposed();
        CappedFloorPlanThresholdMovesToApproved();
        CappedFloorPlanThresholdRejectsMovesFromWithdrawn();
    }
}
