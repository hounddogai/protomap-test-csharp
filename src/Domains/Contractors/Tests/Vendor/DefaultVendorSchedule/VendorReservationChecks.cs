namespace Acme.Workforce.Contractors.Tests.Vendor.DefaultVendorSchedule;

using Acme.Workforce.Contractors.Vendor.DefaultVendorSchedule;

/// <summary>Checks of default vendor schedule code, run by the project's runner.</summary>
public static class VendorReservationChecks
{
    public static void VendorReservationStartsInDraft()
    {
        var item = VendorReservation.NewVendorReservation("code-622", "title-719");
        Check.Equal(item.Status, VendorReservationStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void VendorReservationMovesToActive()
    {
        var item = VendorReservation.NewVendorReservation("code-622", "title-719");
        try
        {
            item.MoveTo(VendorReservationStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, VendorReservationStatus.Active, "status");
    }

    public static void VendorReservationRejectsMovesFromClosed()
    {
        var item = VendorReservation.NewVendorReservation("code-622", "title-719");
        item.Status = VendorReservationStatus.Closed;
        Check.Fails(() => item.MoveTo(VendorReservationStatus.Draft), "move");
    }

    public static void RunAll()
    {
        VendorReservationStartsInDraft();
        VendorReservationMovesToActive();
        VendorReservationRejectsMovesFromClosed();
    }
}
