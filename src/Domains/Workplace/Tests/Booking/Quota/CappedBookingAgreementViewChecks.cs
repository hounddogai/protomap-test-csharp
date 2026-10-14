namespace Acme.Workforce.Workplace.Tests.Booking.Quota;

using Acme.Workforce.Workplace.Booking.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class CappedBookingAgreementViewChecks
{
    public static void CappedBookingAgreementViewShowsTheStatusInCapitals()
    {
        var item = CappedBookingAgreement.NewCappedBookingAgreement("code-442", "name-725");
        var view = CappedBookingAgreementView.ToCappedBookingAgreementView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CappedBookingAgreementView.ToCappedBookingAgreementViews(new List<CappedBookingAgreement> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CappedBookingAgreementViewShowsTheStatusInCapitals();
    }
}
