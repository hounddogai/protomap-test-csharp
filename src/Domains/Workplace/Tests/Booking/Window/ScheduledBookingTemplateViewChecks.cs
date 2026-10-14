namespace Acme.Workforce.Workplace.Tests.Booking.Window;

using Acme.Workforce.Workplace.Booking.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class ScheduledBookingTemplateViewChecks
{
    public static void ScheduledBookingTemplateViewShowsTheStatusInCapitals()
    {
        var item = ScheduledBookingTemplate.NewScheduledBookingTemplate("code-651", "name-222");
        var view = ScheduledBookingTemplateView.ToScheduledBookingTemplateView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ScheduledBookingTemplateView.ToScheduledBookingTemplateViews(new List<ScheduledBookingTemplate> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ScheduledBookingTemplateViewShowsTheStatusInCapitals();
    }
}
