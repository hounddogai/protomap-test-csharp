namespace Acme.Workforce.Workplace.Tests.Booking.Quota;

using Acme.Workforce.Workplace.Booking.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class CappedBookingAgreementChecks
{
    public static void CappedBookingAgreementStartsInRequested()
    {
        var item = CappedBookingAgreement.NewCappedBookingAgreement("code-442", "name-725");
        Check.Equal(item.Status, CappedBookingAgreementStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedBookingAgreementMovesToGranted()
    {
        var item = CappedBookingAgreement.NewCappedBookingAgreement("code-442", "name-725");
        try
        {
            item.MoveTo(CappedBookingAgreementStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedBookingAgreementStatus.Granted, "status");
    }

    public static void CappedBookingAgreementRejectsMovesFromExpired()
    {
        var item = CappedBookingAgreement.NewCappedBookingAgreement("code-442", "name-725");
        item.Status = CappedBookingAgreementStatus.Expired;
        Check.Fails(() => item.MoveTo(CappedBookingAgreementStatus.Requested), "move");
    }

    public static void RunAll()
    {
        CappedBookingAgreementStartsInRequested();
        CappedBookingAgreementMovesToGranted();
        CappedBookingAgreementRejectsMovesFromExpired();
    }
}
