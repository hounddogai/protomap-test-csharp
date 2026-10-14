namespace Acme.Workforce.Workplace.Tests.Booking.BookingAudit;

using Acme.Workforce.Workplace.Booking.BookingAudit;

/// <summary>Checks of booking audit code, run by the project's runner.</summary>
public static class BookingBatchChecks
{
    public static void BookingBatchStartsInIdle()
    {
        var item = BookingBatch.NewBookingBatch("code-95", "label-169");
        Check.Equal(item.Status, BookingBatchStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BookingBatchMovesToWarming()
    {
        var item = BookingBatch.NewBookingBatch("code-95", "label-169");
        try
        {
            item.MoveTo(BookingBatchStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BookingBatchStatus.Warming, "status");
    }

    public static void BookingBatchRejectsMovesFromStopped()
    {
        var item = BookingBatch.NewBookingBatch("code-95", "label-169");
        item.Status = BookingBatchStatus.Stopped;
        Check.Fails(() => item.MoveTo(BookingBatchStatus.Idle), "move");
    }

    public static void RunAll()
    {
        BookingBatchStartsInIdle();
        BookingBatchMovesToWarming();
        BookingBatchRejectsMovesFromStopped();
    }
}
