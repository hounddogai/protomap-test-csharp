namespace Acme.Workforce.Workplace.Tests.FloorPlan.Adjustment;

using Acme.Workforce.Workplace.FloorPlan.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class FloorPlanReservationChecks
{
    public static void FloorPlanReservationStartsInNew()
    {
        var item = FloorPlanReservation.NewFloorPlanReservation("code-977", "label-708");
        Check.Equal(item.Status, FloorPlanReservationStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FloorPlanReservationMovesToFailed()
    {
        var item = FloorPlanReservation.NewFloorPlanReservation("code-977", "label-708");
        try
        {
            item.MoveTo(FloorPlanReservationStatus.Failed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FloorPlanReservationStatus.Failed, "status");
    }

    public static void FloorPlanReservationRejectsMovesFromFailed()
    {
        var item = FloorPlanReservation.NewFloorPlanReservation("code-977", "label-708");
        item.Status = FloorPlanReservationStatus.Failed;
        Check.Fails(() => item.MoveTo(FloorPlanReservationStatus.New), "move");
    }

    public static void RunAll()
    {
        FloorPlanReservationStartsInNew();
        FloorPlanReservationMovesToFailed();
        FloorPlanReservationRejectsMovesFromFailed();
    }
}
