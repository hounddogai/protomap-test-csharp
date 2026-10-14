namespace Acme.Workforce.Contractors.Tests.Vendor.Agreement;

using Acme.Workforce.Contractors.Vendor.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class GlobalVendorReservationChecks
{
    public static void GlobalVendorReservationStartsInPlanned()
    {
        var item = GlobalVendorReservation.NewGlobalVendorReservation("code-33", "label-776");
        Check.Equal(item.Status, GlobalVendorReservationStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GlobalVendorReservationMovesToCompleted()
    {
        var item = GlobalVendorReservation.NewGlobalVendorReservation("code-33", "label-776");
        try
        {
            item.MoveTo(GlobalVendorReservationStatus.Completed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GlobalVendorReservationStatus.Completed, "status");
    }

    public static void GlobalVendorReservationRejectsMovesFromCancelled()
    {
        var item = GlobalVendorReservation.NewGlobalVendorReservation("code-33", "label-776");
        item.Status = GlobalVendorReservationStatus.Cancelled;
        Check.Fails(() => item.MoveTo(GlobalVendorReservationStatus.Planned), "move");
    }

    public static void RunAll()
    {
        GlobalVendorReservationStartsInPlanned();
        GlobalVendorReservationMovesToCompleted();
        GlobalVendorReservationRejectsMovesFromCancelled();
    }
}
