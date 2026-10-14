namespace Acme.Workforce.Workplace.Tests.FloorPlan.Reservation;

using Acme.Workforce.Workplace.FloorPlan.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class StrictFloorPlanBundleChecks
{
    public static void StrictFloorPlanBundleStartsInPlanned()
    {
        var item = StrictFloorPlanBundle.NewStrictFloorPlanBundle("code-528", "name-424");
        Check.Equal(item.Status, StrictFloorPlanBundleStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictFloorPlanBundleMovesToInProgress()
    {
        var item = StrictFloorPlanBundle.NewStrictFloorPlanBundle("code-528", "name-424");
        try
        {
            item.MoveTo(StrictFloorPlanBundleStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictFloorPlanBundleStatus.InProgress, "status");
    }

    public static void StrictFloorPlanBundleRejectsMovesFromCancelled()
    {
        var item = StrictFloorPlanBundle.NewStrictFloorPlanBundle("code-528", "name-424");
        item.Status = StrictFloorPlanBundleStatus.Cancelled;
        Check.Fails(() => item.MoveTo(StrictFloorPlanBundleStatus.Planned), "move");
    }

    public static void RunAll()
    {
        StrictFloorPlanBundleStartsInPlanned();
        StrictFloorPlanBundleMovesToInProgress();
        StrictFloorPlanBundleRejectsMovesFromCancelled();
    }
}
