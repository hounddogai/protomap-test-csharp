namespace Acme.Workforce.Workplace.Tests.Booking.BookingVariance;

using Acme.Workforce.Workplace.Booking.BookingVariance;

/// <summary>Checks of booking variance code, run by the project's runner.</summary>
public static class LegacyBookingAuditViewChecks
{
    public static void LegacyBookingAuditViewShowsTheStatusInCapitals()
    {
        var item = LegacyBookingAudit.NewLegacyBookingAudit("code-847", "title-109");
        var view = LegacyBookingAuditView.ToLegacyBookingAuditView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LegacyBookingAuditView.ToLegacyBookingAuditViews(new List<LegacyBookingAudit> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LegacyBookingAuditViewShowsTheStatusInCapitals();
    }
}
