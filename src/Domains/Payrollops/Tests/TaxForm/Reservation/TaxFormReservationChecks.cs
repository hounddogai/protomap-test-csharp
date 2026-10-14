namespace Acme.Workforce.Payrollops.Tests.TaxForm.Reservation;

using Acme.Workforce.Payrollops.TaxForm.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class TaxFormReservationChecks
{
    public static void TaxFormReservationStartsInRequested()
    {
        var item = TaxFormReservation.NewTaxFormReservation("code-360", "label-888");
        Check.Equal(item.Status, TaxFormReservationStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TaxFormReservationMovesToGranted()
    {
        var item = TaxFormReservation.NewTaxFormReservation("code-360", "label-888");
        try
        {
            item.MoveTo(TaxFormReservationStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TaxFormReservationStatus.Granted, "status");
    }

    public static void TaxFormReservationRejectsMovesFromExpired()
    {
        var item = TaxFormReservation.NewTaxFormReservation("code-360", "label-888");
        item.Status = TaxFormReservationStatus.Expired;
        Check.Fails(() => item.MoveTo(TaxFormReservationStatus.Requested), "move");
    }

    public static void RunAll()
    {
        TaxFormReservationStartsInRequested();
        TaxFormReservationMovesToGranted();
        TaxFormReservationRejectsMovesFromExpired();
    }
}
