namespace Acme.Workforce.Workplace.Tests.FloorPlan.Allocation;

using Acme.Workforce.Workplace.FloorPlan.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class FloorPlanBundleChecks
{
    public static void FloorPlanBundleStartsInNew()
    {
        var item = FloorPlanBundle.NewFloorPlanBundle("code-845", "title-940");
        Check.Equal(item.Status, FloorPlanBundleStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FloorPlanBundleMovesToDone()
    {
        var item = FloorPlanBundle.NewFloorPlanBundle("code-845", "title-940");
        try
        {
            item.MoveTo(FloorPlanBundleStatus.Done);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FloorPlanBundleStatus.Done, "status");
    }

    public static void FloorPlanBundleRejectsMovesFromFailed()
    {
        var item = FloorPlanBundle.NewFloorPlanBundle("code-845", "title-940");
        item.Status = FloorPlanBundleStatus.Failed;
        Check.Fails(() => item.MoveTo(FloorPlanBundleStatus.New), "move");
    }

    public static void RunAll()
    {
        FloorPlanBundleStartsInNew();
        FloorPlanBundleMovesToDone();
        FloorPlanBundleRejectsMovesFromFailed();
    }
}
