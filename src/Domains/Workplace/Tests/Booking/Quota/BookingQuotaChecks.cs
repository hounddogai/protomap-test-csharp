namespace Acme.Workforce.Workplace.Tests.Booking.Quota;

using Acme.Workforce.Workplace.Booking.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class BookingQuotaChecks
{
    public static void BookingQuotaStartsInOpen()
    {
        var item = BookingQuota.NewBookingQuota("code-12", "title-434");
        Check.Equal(item.Status, BookingQuotaStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BookingQuotaMovesToInReview()
    {
        var item = BookingQuota.NewBookingQuota("code-12", "title-434");
        try
        {
            item.MoveTo(BookingQuotaStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BookingQuotaStatus.InReview, "status");
    }

    public static void BookingQuotaRejectsMovesFromArchived()
    {
        var item = BookingQuota.NewBookingQuota("code-12", "title-434");
        item.Status = BookingQuotaStatus.Archived;
        Check.Fails(() => item.MoveTo(BookingQuotaStatus.Open), "move");
    }

    public static void RunAll()
    {
        BookingQuotaStartsInOpen();
        BookingQuotaMovesToInReview();
        BookingQuotaRejectsMovesFromArchived();
    }
}
