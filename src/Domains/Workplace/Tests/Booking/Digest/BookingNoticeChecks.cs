namespace Acme.Workforce.Workplace.Tests.Booking.Digest;

using Acme.Workforce.Workplace.Booking.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class BookingNoticeChecks
{
    public static void BookingNoticeStartsInNew()
    {
        var item = BookingNotice.NewBookingNotice("code-369", "title-778");
        Check.Equal(item.Status, BookingNoticeStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BookingNoticeMovesToRunning()
    {
        var item = BookingNotice.NewBookingNotice("code-369", "title-778");
        try
        {
            item.MoveTo(BookingNoticeStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BookingNoticeStatus.Running, "status");
    }

    public static void BookingNoticeRejectsMovesFromFailed()
    {
        var item = BookingNotice.NewBookingNotice("code-369", "title-778");
        item.Status = BookingNoticeStatus.Failed;
        Check.Fails(() => item.MoveTo(BookingNoticeStatus.New), "move");
    }

    public static void RunAll()
    {
        BookingNoticeStartsInNew();
        BookingNoticeMovesToRunning();
        BookingNoticeRejectsMovesFromFailed();
    }
}
