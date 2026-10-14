namespace Acme.Workforce.Workplace.Tests.Booking.Digest;

using Acme.Workforce.Workplace.Booking.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class CappedBookingPreferenceViewChecks
{
    public static void CappedBookingPreferenceViewShowsTheStatusInCapitals()
    {
        var item = CappedBookingPreference.NewCappedBookingPreference("code-162", "label-875");
        var view = CappedBookingPreferenceView.ToCappedBookingPreferenceView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CappedBookingPreferenceView.ToCappedBookingPreferenceViews(new List<CappedBookingPreference> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CappedBookingPreferenceViewShowsTheStatusInCapitals();
    }
}
