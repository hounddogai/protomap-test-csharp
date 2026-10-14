namespace Acme.Workforce.Assets.Tests.Refresh.Reservation;

using Acme.Workforce.Assets.Refresh.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class ManualRefreshReservationChecks
{
    public static void ManualRefreshReservationStartsInCreated()
    {
        var item = ManualRefreshReservation.NewManualRefreshReservation("code-4", "name-590");
        Check.Equal(item.Status, ManualRefreshReservationStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ManualRefreshReservationMovesToRetired()
    {
        var item = ManualRefreshReservation.NewManualRefreshReservation("code-4", "name-590");
        try
        {
            item.MoveTo(ManualRefreshReservationStatus.Retired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ManualRefreshReservationStatus.Retired, "status");
    }

    public static void ManualRefreshReservationRejectsMovesFromRetired()
    {
        var item = ManualRefreshReservation.NewManualRefreshReservation("code-4", "name-590");
        item.Status = ManualRefreshReservationStatus.Retired;
        Check.Fails(() => item.MoveTo(ManualRefreshReservationStatus.Created), "move");
    }

    public static void RunAll()
    {
        ManualRefreshReservationStartsInCreated();
        ManualRefreshReservationMovesToRetired();
        ManualRefreshReservationRejectsMovesFromRetired();
    }
}
