namespace Acme.Workforce.Workplace.Tests.FloorPlan.Entry;

using Acme.Workforce.Workplace.FloorPlan.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class ManualFloorPlanReservationChecks
{
    public static void ManualFloorPlanReservationStartsInProposed()
    {
        var item = ManualFloorPlanReservation.NewManualFloorPlanReservation("code-741", "name-666");
        Check.Equal(item.Status, ManualFloorPlanReservationStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ManualFloorPlanReservationMovesToWithdrawn()
    {
        var item = ManualFloorPlanReservation.NewManualFloorPlanReservation("code-741", "name-666");
        try
        {
            item.MoveTo(ManualFloorPlanReservationStatus.Withdrawn);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ManualFloorPlanReservationStatus.Withdrawn, "status");
    }

    public static void ManualFloorPlanReservationRejectsMovesFromWithdrawn()
    {
        var item = ManualFloorPlanReservation.NewManualFloorPlanReservation("code-741", "name-666");
        item.Status = ManualFloorPlanReservationStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(ManualFloorPlanReservationStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        ManualFloorPlanReservationStartsInProposed();
        ManualFloorPlanReservationMovesToWithdrawn();
        ManualFloorPlanReservationRejectsMovesFromWithdrawn();
    }
}
