namespace Acme.Workforce.Assets.Tests.LicenseSeat.StrictSeatReservation;

using Acme.Workforce.Assets.LicenseSeat.StrictSeatReservation;

/// <summary>Checks of strict seat reservation code, run by the project's runner.</summary>
public static class TieredLicenseSeatReservationChecks
{
    public static void TieredLicenseSeatReservationStartsInProposed()
    {
        var item = TieredLicenseSeatReservation.NewTieredLicenseSeatReservation("code-792", "label-893");
        Check.Equal(item.Status, TieredLicenseSeatReservationStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredLicenseSeatReservationMovesToRejected()
    {
        var item = TieredLicenseSeatReservation.NewTieredLicenseSeatReservation("code-792", "label-893");
        try
        {
            item.MoveTo(TieredLicenseSeatReservationStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredLicenseSeatReservationStatus.Rejected, "status");
    }

    public static void TieredLicenseSeatReservationRejectsMovesFromWithdrawn()
    {
        var item = TieredLicenseSeatReservation.NewTieredLicenseSeatReservation("code-792", "label-893");
        item.Status = TieredLicenseSeatReservationStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(TieredLicenseSeatReservationStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        TieredLicenseSeatReservationStartsInProposed();
        TieredLicenseSeatReservationMovesToRejected();
        TieredLicenseSeatReservationRejectsMovesFromWithdrawn();
    }
}
