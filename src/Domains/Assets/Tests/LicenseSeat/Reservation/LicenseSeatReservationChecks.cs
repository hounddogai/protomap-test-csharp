namespace Acme.Workforce.Assets.Tests.LicenseSeat.Reservation;

using Acme.Workforce.Assets.LicenseSeat.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class LicenseSeatReservationChecks
{
    public static void LicenseSeatReservationStartsInProposed()
    {
        var item = LicenseSeatReservation.NewLicenseSeatReservation("code-999", "title-43");
        Check.Equal(item.Status, LicenseSeatReservationStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LicenseSeatReservationMovesToWithdrawn()
    {
        var item = LicenseSeatReservation.NewLicenseSeatReservation("code-999", "title-43");
        try
        {
            item.MoveTo(LicenseSeatReservationStatus.Withdrawn);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LicenseSeatReservationStatus.Withdrawn, "status");
    }

    public static void LicenseSeatReservationRejectsMovesFromWithdrawn()
    {
        var item = LicenseSeatReservation.NewLicenseSeatReservation("code-999", "title-43");
        item.Status = LicenseSeatReservationStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(LicenseSeatReservationStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        LicenseSeatReservationStartsInProposed();
        LicenseSeatReservationMovesToWithdrawn();
        LicenseSeatReservationRejectsMovesFromWithdrawn();
    }
}
