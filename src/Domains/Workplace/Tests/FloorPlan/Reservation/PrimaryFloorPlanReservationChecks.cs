namespace Acme.Workforce.Workplace.Tests.FloorPlan.Reservation;

using Acme.Workforce.Workplace.FloorPlan.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class PrimaryFloorPlanReservationChecks
{
    public static void PrimaryFloorPlanReservationStartsInNew()
    {
        var item = PrimaryFloorPlanReservation.NewPrimaryFloorPlanReservation("code-666", "name-334");
        Check.Equal(item.Status, PrimaryFloorPlanReservationStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PrimaryFloorPlanReservationMovesToDone()
    {
        var item = PrimaryFloorPlanReservation.NewPrimaryFloorPlanReservation("code-666", "name-334");
        try
        {
            item.MoveTo(PrimaryFloorPlanReservationStatus.Done);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PrimaryFloorPlanReservationStatus.Done, "status");
    }

    public static void PrimaryFloorPlanReservationRejectsMovesFromFailed()
    {
        var item = PrimaryFloorPlanReservation.NewPrimaryFloorPlanReservation("code-666", "name-334");
        item.Status = PrimaryFloorPlanReservationStatus.Failed;
        Check.Fails(() => item.MoveTo(PrimaryFloorPlanReservationStatus.New), "move");
    }

    public static void RunAll()
    {
        PrimaryFloorPlanReservationStartsInNew();
        PrimaryFloorPlanReservationMovesToDone();
        PrimaryFloorPlanReservationRejectsMovesFromFailed();
    }
}
