namespace Acme.Workforce.Workplace.Tests.Booking.Review;

using Acme.Workforce.Workplace.Booking.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class WeightedBookingEntryChecks
{
    public static void WeightedBookingEntryStartsInDraft()
    {
        var item = WeightedBookingEntry.NewWeightedBookingEntry("code-213", "name-292");
        Check.Equal(item.Status, WeightedBookingEntryStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WeightedBookingEntryMovesToActive()
    {
        var item = WeightedBookingEntry.NewWeightedBookingEntry("code-213", "name-292");
        try
        {
            item.MoveTo(WeightedBookingEntryStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WeightedBookingEntryStatus.Active, "status");
    }

    public static void WeightedBookingEntryRejectsMovesFromClosed()
    {
        var item = WeightedBookingEntry.NewWeightedBookingEntry("code-213", "name-292");
        item.Status = WeightedBookingEntryStatus.Closed;
        Check.Fails(() => item.MoveTo(WeightedBookingEntryStatus.Draft), "move");
    }

    public static void RunAll()
    {
        WeightedBookingEntryStartsInDraft();
        WeightedBookingEntryMovesToActive();
        WeightedBookingEntryRejectsMovesFromClosed();
    }
}
