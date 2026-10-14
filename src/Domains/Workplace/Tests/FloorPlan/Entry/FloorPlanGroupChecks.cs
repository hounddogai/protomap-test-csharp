namespace Acme.Workforce.Workplace.Tests.FloorPlan.Entry;

using Acme.Workforce.Workplace.FloorPlan.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class FloorPlanGroupChecks
{
    public static void FloorPlanGroupStartsInCreated()
    {
        var item = FloorPlanGroup.NewFloorPlanGroup("code-5", "label-78");
        Check.Equal(item.Status, FloorPlanGroupStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FloorPlanGroupMovesToPublished()
    {
        var item = FloorPlanGroup.NewFloorPlanGroup("code-5", "label-78");
        try
        {
            item.MoveTo(FloorPlanGroupStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FloorPlanGroupStatus.Published, "status");
    }

    public static void FloorPlanGroupRejectsMovesFromRetired()
    {
        var item = FloorPlanGroup.NewFloorPlanGroup("code-5", "label-78");
        item.Status = FloorPlanGroupStatus.Retired;
        Check.Fails(() => item.MoveTo(FloorPlanGroupStatus.Created), "move");
    }

    public static void RunAll()
    {
        FloorPlanGroupStartsInCreated();
        FloorPlanGroupMovesToPublished();
        FloorPlanGroupRejectsMovesFromRetired();
    }
}
