namespace Acme.Workforce.Workplace.Tests.Booking.Quota;

using Acme.Workforce.Workplace.Booking.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class AdaptiveBookingProfileViewChecks
{
    public static void AdaptiveBookingProfileViewShowsTheStatusInCapitals()
    {
        var item = AdaptiveBookingProfile.NewAdaptiveBookingProfile("code-618", "label-321");
        var view = AdaptiveBookingProfileView.ToAdaptiveBookingProfileView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AdaptiveBookingProfileView.ToAdaptiveBookingProfileViews(new List<AdaptiveBookingProfile> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AdaptiveBookingProfileViewShowsTheStatusInCapitals();
    }
}
