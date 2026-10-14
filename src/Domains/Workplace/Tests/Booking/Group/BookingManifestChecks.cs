namespace Acme.Workforce.Workplace.Tests.Booking.Group;

using Acme.Workforce.Workplace.Booking.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class BookingManifestChecks
{
    public static void BookingManifestStartsInNew()
    {
        var item = BookingManifest.NewBookingManifest("code-154", "name-5");
        Check.Equal(item.Status, BookingManifestStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BookingManifestMovesToQueued()
    {
        var item = BookingManifest.NewBookingManifest("code-154", "name-5");
        try
        {
            item.MoveTo(BookingManifestStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BookingManifestStatus.Queued, "status");
    }

    public static void BookingManifestRejectsMovesFromFailed()
    {
        var item = BookingManifest.NewBookingManifest("code-154", "name-5");
        item.Status = BookingManifestStatus.Failed;
        Check.Fails(() => item.MoveTo(BookingManifestStatus.New), "move");
    }

    public static void RunAll()
    {
        BookingManifestStartsInNew();
        BookingManifestMovesToQueued();
        BookingManifestRejectsMovesFromFailed();
    }
}
