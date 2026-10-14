namespace Acme.Workforce.Workplace.Tests.FloorPlan.Threshold;

using Acme.Workforce.Workplace.FloorPlan.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class ScheduledFloorPlanGroupChecks
{
    public static void ScheduledFloorPlanGroupStartsInCreated()
    {
        var item = ScheduledFloorPlanGroup.NewScheduledFloorPlanGroup("code-593", "title-230");
        Check.Equal(item.Status, ScheduledFloorPlanGroupStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledFloorPlanGroupMovesToValidated()
    {
        var item = ScheduledFloorPlanGroup.NewScheduledFloorPlanGroup("code-593", "title-230");
        try
        {
            item.MoveTo(ScheduledFloorPlanGroupStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledFloorPlanGroupStatus.Validated, "status");
    }

    public static void ScheduledFloorPlanGroupRejectsMovesFromRetired()
    {
        var item = ScheduledFloorPlanGroup.NewScheduledFloorPlanGroup("code-593", "title-230");
        item.Status = ScheduledFloorPlanGroupStatus.Retired;
        Check.Fails(() => item.MoveTo(ScheduledFloorPlanGroupStatus.Created), "move");
    }

    public static void RunAll()
    {
        ScheduledFloorPlanGroupStartsInCreated();
        ScheduledFloorPlanGroupMovesToValidated();
        ScheduledFloorPlanGroupRejectsMovesFromRetired();
    }
}
