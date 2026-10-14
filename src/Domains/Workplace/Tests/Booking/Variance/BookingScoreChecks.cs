namespace Acme.Workforce.Workplace.Tests.Booking.Variance;

using Acme.Workforce.Workplace.Booking.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class BookingScoreChecks
{
    public static void BookingScoreStartsInRequested()
    {
        var item = BookingScore.NewBookingScore("code-413", "label-301");
        Check.Equal(item.Status, BookingScoreStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BookingScoreMovesToRevoked()
    {
        var item = BookingScore.NewBookingScore("code-413", "label-301");
        try
        {
            item.MoveTo(BookingScoreStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BookingScoreStatus.Revoked, "status");
    }

    public static void BookingScoreRejectsMovesFromExpired()
    {
        var item = BookingScore.NewBookingScore("code-413", "label-301");
        item.Status = BookingScoreStatus.Expired;
        Check.Fails(() => item.MoveTo(BookingScoreStatus.Requested), "move");
    }

    public static void RunAll()
    {
        BookingScoreStartsInRequested();
        BookingScoreMovesToRevoked();
        BookingScoreRejectsMovesFromExpired();
    }
}
