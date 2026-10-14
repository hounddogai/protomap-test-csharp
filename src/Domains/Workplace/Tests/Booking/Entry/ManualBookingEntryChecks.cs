namespace Acme.Workforce.Workplace.Tests.Booking.Entry;

using Acme.Workforce.Workplace.Booking.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class ManualBookingEntryChecks
{
    public static void ManualBookingEntryStartsInCreated()
    {
        var item = ManualBookingEntry.NewManualBookingEntry("code-63", "title-135");
        Check.Equal(item.Status, ManualBookingEntryStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ManualBookingEntryMovesToValidated()
    {
        var item = ManualBookingEntry.NewManualBookingEntry("code-63", "title-135");
        try
        {
            item.MoveTo(ManualBookingEntryStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ManualBookingEntryStatus.Validated, "status");
    }

    public static void ManualBookingEntryRejectsMovesFromRetired()
    {
        var item = ManualBookingEntry.NewManualBookingEntry("code-63", "title-135");
        item.Status = ManualBookingEntryStatus.Retired;
        Check.Fails(() => item.MoveTo(ManualBookingEntryStatus.Created), "move");
    }

    public static void RunAll()
    {
        ManualBookingEntryStartsInCreated();
        ManualBookingEntryMovesToValidated();
        ManualBookingEntryRejectsMovesFromRetired();
    }
}
