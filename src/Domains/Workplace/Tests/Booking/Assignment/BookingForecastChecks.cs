namespace Acme.Workforce.Workplace.Tests.Booking.Assignment;

using Acme.Workforce.Workplace.Booking.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class BookingForecastChecks
{
    public static void BookingForecastStartsInIdle()
    {
        var item = BookingForecast.NewBookingForecast("code-62", "title-438");
        Check.Equal(item.Status, BookingForecastStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BookingForecastMovesToDraining()
    {
        var item = BookingForecast.NewBookingForecast("code-62", "title-438");
        try
        {
            item.MoveTo(BookingForecastStatus.Draining);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BookingForecastStatus.Draining, "status");
    }

    public static void BookingForecastRejectsMovesFromStopped()
    {
        var item = BookingForecast.NewBookingForecast("code-62", "title-438");
        item.Status = BookingForecastStatus.Stopped;
        Check.Fails(() => item.MoveTo(BookingForecastStatus.Idle), "move");
    }

    public static void RunAll()
    {
        BookingForecastStartsInIdle();
        BookingForecastMovesToDraining();
        BookingForecastRejectsMovesFromStopped();
    }
}
