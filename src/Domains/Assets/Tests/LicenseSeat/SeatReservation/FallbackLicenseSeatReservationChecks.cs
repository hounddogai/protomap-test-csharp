namespace Acme.Workforce.Assets.Tests.LicenseSeat.SeatReservation;

using Acme.Workforce.Assets.LicenseSeat.SeatReservation;

/// <summary>Checks of seat reservation code, run by the project's runner.</summary>
public static class FallbackLicenseSeatReservationChecks
{
    public static void FallbackLicenseSeatReservationStartsInProposed()
    {
        var item = FallbackLicenseSeatReservation.NewFallbackLicenseSeatReservation("code-285", "name-192");
        Check.Equal(item.Status, FallbackLicenseSeatReservationStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackLicenseSeatReservationMovesToApproved()
    {
        var item = FallbackLicenseSeatReservation.NewFallbackLicenseSeatReservation("code-285", "name-192");
        try
        {
            item.MoveTo(FallbackLicenseSeatReservationStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackLicenseSeatReservationStatus.Approved, "status");
    }

    public static void FallbackLicenseSeatReservationRejectsMovesFromWithdrawn()
    {
        var item = FallbackLicenseSeatReservation.NewFallbackLicenseSeatReservation("code-285", "name-192");
        item.Status = FallbackLicenseSeatReservationStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(FallbackLicenseSeatReservationStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        FallbackLicenseSeatReservationStartsInProposed();
        FallbackLicenseSeatReservationMovesToApproved();
        FallbackLicenseSeatReservationRejectsMovesFromWithdrawn();
    }
}
