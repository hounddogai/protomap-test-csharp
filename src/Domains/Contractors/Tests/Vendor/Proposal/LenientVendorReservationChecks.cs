namespace Acme.Workforce.Contractors.Tests.Vendor.Proposal;

using Acme.Workforce.Contractors.Vendor.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class LenientVendorReservationChecks
{
    public static void LenientVendorReservationStartsInCreated()
    {
        var item = LenientVendorReservation.NewLenientVendorReservation("code-153", "name-378");
        Check.Equal(item.Status, LenientVendorReservationStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientVendorReservationMovesToValidated()
    {
        var item = LenientVendorReservation.NewLenientVendorReservation("code-153", "name-378");
        try
        {
            item.MoveTo(LenientVendorReservationStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientVendorReservationStatus.Validated, "status");
    }

    public static void LenientVendorReservationRejectsMovesFromRetired()
    {
        var item = LenientVendorReservation.NewLenientVendorReservation("code-153", "name-378");
        item.Status = LenientVendorReservationStatus.Retired;
        Check.Fails(() => item.MoveTo(LenientVendorReservationStatus.Created), "move");
    }

    public static void RunAll()
    {
        LenientVendorReservationStartsInCreated();
        LenientVendorReservationMovesToValidated();
        LenientVendorReservationRejectsMovesFromRetired();
    }
}
