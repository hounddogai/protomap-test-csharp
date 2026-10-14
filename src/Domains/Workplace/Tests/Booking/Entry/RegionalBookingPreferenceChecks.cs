namespace Acme.Workforce.Workplace.Tests.Booking.Entry;

using Acme.Workforce.Workplace.Booking.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class RegionalBookingPreferenceChecks
{
    public static void RegionalBookingPreferenceStartsInPlanned()
    {
        var item = RegionalBookingPreference.NewRegionalBookingPreference("code-562", "label-881");
        Check.Equal(item.Status, RegionalBookingPreferenceStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RegionalBookingPreferenceMovesToScheduled()
    {
        var item = RegionalBookingPreference.NewRegionalBookingPreference("code-562", "label-881");
        try
        {
            item.MoveTo(RegionalBookingPreferenceStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RegionalBookingPreferenceStatus.Scheduled, "status");
    }

    public static void RegionalBookingPreferenceRejectsMovesFromCancelled()
    {
        var item = RegionalBookingPreference.NewRegionalBookingPreference("code-562", "label-881");
        item.Status = RegionalBookingPreferenceStatus.Cancelled;
        Check.Fails(() => item.MoveTo(RegionalBookingPreferenceStatus.Planned), "move");
    }

    public static void RunAll()
    {
        RegionalBookingPreferenceStartsInPlanned();
        RegionalBookingPreferenceMovesToScheduled();
        RegionalBookingPreferenceRejectsMovesFromCancelled();
    }
}
