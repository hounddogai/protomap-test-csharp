namespace Acme.Workforce.Assets.Tests.Warranty.Reservation;

using Acme.Workforce.Assets.Warranty.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class WarrantyReservationChecks
{
    public static void WarrantyReservationStartsInRequested()
    {
        var item = WarrantyReservation.NewWarrantyReservation("code-254", "title-140");
        Check.Equal(item.Status, WarrantyReservationStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WarrantyReservationMovesToRevoked()
    {
        var item = WarrantyReservation.NewWarrantyReservation("code-254", "title-140");
        try
        {
            item.MoveTo(WarrantyReservationStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WarrantyReservationStatus.Revoked, "status");
    }

    public static void WarrantyReservationRejectsMovesFromExpired()
    {
        var item = WarrantyReservation.NewWarrantyReservation("code-254", "title-140");
        item.Status = WarrantyReservationStatus.Expired;
        Check.Fails(() => item.MoveTo(WarrantyReservationStatus.Requested), "move");
    }

    public static void RunAll()
    {
        WarrantyReservationStartsInRequested();
        WarrantyReservationMovesToRevoked();
        WarrantyReservationRejectsMovesFromExpired();
    }
}
