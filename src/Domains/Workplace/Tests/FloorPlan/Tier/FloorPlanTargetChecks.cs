namespace Acme.Workforce.Workplace.Tests.FloorPlan.Tier;

using Acme.Workforce.Workplace.FloorPlan.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class FloorPlanTargetChecks
{
    public static void FloorPlanTargetStartsInCreated()
    {
        var item = FloorPlanTarget.NewFloorPlanTarget("code-778", "title-971");
        Check.Equal(item.Status, FloorPlanTargetStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FloorPlanTargetMovesToPublished()
    {
        var item = FloorPlanTarget.NewFloorPlanTarget("code-778", "title-971");
        try
        {
            item.MoveTo(FloorPlanTargetStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FloorPlanTargetStatus.Published, "status");
    }

    public static void FloorPlanTargetRejectsMovesFromRetired()
    {
        var item = FloorPlanTarget.NewFloorPlanTarget("code-778", "title-971");
        item.Status = FloorPlanTargetStatus.Retired;
        Check.Fails(() => item.MoveTo(FloorPlanTargetStatus.Created), "move");
    }

    public static void RunAll()
    {
        FloorPlanTargetStartsInCreated();
        FloorPlanTargetMovesToPublished();
        FloorPlanTargetRejectsMovesFromRetired();
    }
}
