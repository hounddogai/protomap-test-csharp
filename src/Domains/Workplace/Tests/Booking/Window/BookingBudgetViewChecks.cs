namespace Acme.Workforce.Workplace.Tests.Booking.Window;

using Acme.Workforce.Workplace.Booking.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class BookingBudgetViewChecks
{
    public static void BookingBudgetViewShowsTheStatusInCapitals()
    {
        var item = BookingBudget.NewBookingBudget("code-305", "label-878");
        var view = BookingBudgetView.ToBookingBudgetView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BookingBudgetView.ToBookingBudgetViews(new List<BookingBudget> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BookingBudgetViewShowsTheStatusInCapitals();
    }
}
