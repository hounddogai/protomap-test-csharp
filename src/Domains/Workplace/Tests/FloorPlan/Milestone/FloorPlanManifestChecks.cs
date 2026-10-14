namespace Acme.Workforce.Workplace.Tests.FloorPlan.Milestone;

using Acme.Workforce.Workplace.FloorPlan.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class FloorPlanManifestChecks
{
    public static void FloorPlanManifestStartsInProposed()
    {
        var item = FloorPlanManifest.NewFloorPlanManifest("code-20", "title-279");
        Check.Equal(item.Status, FloorPlanManifestStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FloorPlanManifestMovesToRejected()
    {
        var item = FloorPlanManifest.NewFloorPlanManifest("code-20", "title-279");
        try
        {
            item.MoveTo(FloorPlanManifestStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FloorPlanManifestStatus.Rejected, "status");
    }

    public static void FloorPlanManifestRejectsMovesFromWithdrawn()
    {
        var item = FloorPlanManifest.NewFloorPlanManifest("code-20", "title-279");
        item.Status = FloorPlanManifestStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(FloorPlanManifestStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        FloorPlanManifestStartsInProposed();
        FloorPlanManifestMovesToRejected();
        FloorPlanManifestRejectsMovesFromWithdrawn();
    }
}
