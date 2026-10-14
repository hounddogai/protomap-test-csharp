namespace Acme.Workforce.Workplace.Tests.Occupancy.Reservation;

using Acme.Workforce.Workplace.Occupancy.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class OccupancyReservationChecks
{
    public static void OccupancyReservationStartsInCreated()
    {
        var item = OccupancyReservation.NewOccupancyReservation("code-934", "title-972");
        Check.Equal(item.Status, OccupancyReservationStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void OccupancyReservationMovesToValidated()
    {
        var item = OccupancyReservation.NewOccupancyReservation("code-934", "title-972");
        try
        {
            item.MoveTo(OccupancyReservationStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, OccupancyReservationStatus.Validated, "status");
    }

    public static void OccupancyReservationRejectsMovesFromRetired()
    {
        var item = OccupancyReservation.NewOccupancyReservation("code-934", "title-972");
        item.Status = OccupancyReservationStatus.Retired;
        Check.Fails(() => item.MoveTo(OccupancyReservationStatus.Created), "move");
    }

    public static void RunAll()
    {
        OccupancyReservationStartsInCreated();
        OccupancyReservationMovesToValidated();
        OccupancyReservationRejectsMovesFromRetired();
    }
}
