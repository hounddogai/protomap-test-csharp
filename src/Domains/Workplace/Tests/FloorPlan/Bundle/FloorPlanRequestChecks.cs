namespace Acme.Workforce.Workplace.Tests.FloorPlan.Bundle;

using Acme.Workforce.Workplace.FloorPlan.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class FloorPlanRequestChecks
{
    public static void FloorPlanRequestStartsInProposed()
    {
        var item = FloorPlanRequest.NewFloorPlanRequest("code-524", "label-861");
        Check.Equal(item.Status, FloorPlanRequestStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FloorPlanRequestMovesToApproved()
    {
        var item = FloorPlanRequest.NewFloorPlanRequest("code-524", "label-861");
        try
        {
            item.MoveTo(FloorPlanRequestStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FloorPlanRequestStatus.Approved, "status");
    }

    public static void FloorPlanRequestRejectsMovesFromWithdrawn()
    {
        var item = FloorPlanRequest.NewFloorPlanRequest("code-524", "label-861");
        item.Status = FloorPlanRequestStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(FloorPlanRequestStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        FloorPlanRequestStartsInProposed();
        FloorPlanRequestMovesToApproved();
        FloorPlanRequestRejectsMovesFromWithdrawn();
    }
}
