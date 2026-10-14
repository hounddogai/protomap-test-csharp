namespace Acme.Workforce.Workplace.Tests.FloorPlan.Threshold;

using Acme.Workforce.Workplace.FloorPlan.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class FloorPlanClaimChecks
{
    public static void FloorPlanClaimStartsInPlanned()
    {
        var item = FloorPlanClaim.NewFloorPlanClaim("code-445", "label-512");
        Check.Equal(item.Status, FloorPlanClaimStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FloorPlanClaimMovesToScheduled()
    {
        var item = FloorPlanClaim.NewFloorPlanClaim("code-445", "label-512");
        try
        {
            item.MoveTo(FloorPlanClaimStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FloorPlanClaimStatus.Scheduled, "status");
    }

    public static void FloorPlanClaimRejectsMovesFromCancelled()
    {
        var item = FloorPlanClaim.NewFloorPlanClaim("code-445", "label-512");
        item.Status = FloorPlanClaimStatus.Cancelled;
        Check.Fails(() => item.MoveTo(FloorPlanClaimStatus.Planned), "move");
    }

    public static void RunAll()
    {
        FloorPlanClaimStartsInPlanned();
        FloorPlanClaimMovesToScheduled();
        FloorPlanClaimRejectsMovesFromCancelled();
    }
}
