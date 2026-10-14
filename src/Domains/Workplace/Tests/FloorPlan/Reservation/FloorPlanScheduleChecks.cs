namespace Acme.Workforce.Workplace.Tests.FloorPlan.Reservation;

using Acme.Workforce.Workplace.FloorPlan.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class FloorPlanScheduleChecks
{
    public static void FloorPlanScheduleStartsInOpen()
    {
        var item = FloorPlanSchedule.NewFloorPlanSchedule("code-891", "name-249");
        Check.Equal(item.Status, FloorPlanScheduleStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FloorPlanScheduleMovesToInReview()
    {
        var item = FloorPlanSchedule.NewFloorPlanSchedule("code-891", "name-249");
        try
        {
            item.MoveTo(FloorPlanScheduleStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FloorPlanScheduleStatus.InReview, "status");
    }

    public static void FloorPlanScheduleRejectsMovesFromArchived()
    {
        var item = FloorPlanSchedule.NewFloorPlanSchedule("code-891", "name-249");
        item.Status = FloorPlanScheduleStatus.Archived;
        Check.Fails(() => item.MoveTo(FloorPlanScheduleStatus.Open), "move");
    }

    public static void RunAll()
    {
        FloorPlanScheduleStartsInOpen();
        FloorPlanScheduleMovesToInReview();
        FloorPlanScheduleRejectsMovesFromArchived();
    }
}
