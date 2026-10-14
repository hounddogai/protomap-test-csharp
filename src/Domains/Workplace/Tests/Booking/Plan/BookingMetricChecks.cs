namespace Acme.Workforce.Workplace.Tests.Booking.Plan;

using Acme.Workforce.Workplace.Booking.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class BookingMetricChecks
{
    public static void BookingMetricStartsInRequested()
    {
        var item = BookingMetric.NewBookingMetric("code-748", "name-865");
        Check.Equal(item.Status, BookingMetricStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BookingMetricMovesToExpired()
    {
        var item = BookingMetric.NewBookingMetric("code-748", "name-865");
        try
        {
            item.MoveTo(BookingMetricStatus.Expired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BookingMetricStatus.Expired, "status");
    }

    public static void BookingMetricRejectsMovesFromExpired()
    {
        var item = BookingMetric.NewBookingMetric("code-748", "name-865");
        item.Status = BookingMetricStatus.Expired;
        Check.Fails(() => item.MoveTo(BookingMetricStatus.Requested), "move");
    }

    public static void RunAll()
    {
        BookingMetricStartsInRequested();
        BookingMetricMovesToExpired();
        BookingMetricRejectsMovesFromExpired();
    }
}
