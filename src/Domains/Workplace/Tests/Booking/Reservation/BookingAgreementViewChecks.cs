namespace Acme.Workforce.Workplace.Tests.Booking.Reservation;

using Acme.Workforce.Workplace.Booking.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class BookingAgreementViewChecks
{
    public static void BookingAgreementViewShowsTheStatusInCapitals()
    {
        var item = BookingAgreement.NewBookingAgreement("code-870", "name-657");
        var view = BookingAgreementView.ToBookingAgreementView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BookingAgreementView.ToBookingAgreementViews(new List<BookingAgreement> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BookingAgreementViewShowsTheStatusInCapitals();
    }
}
