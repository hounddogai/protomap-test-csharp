namespace Acme.Workforce.Absence.Tests.Holiday.HolidayReservation;

using Acme.Workforce.Absence.Holiday.HolidayReservation;

/// <summary>Checks of holiday reservation code, run by the project's runner.</summary>
public static class RegionalHolidayWindowChecks
{
    public static void RegionalHolidayWindowStartsInRequested()
    {
        var item = RegionalHolidayWindow.NewRegionalHolidayWindow("code-705", "label-811");
        Check.Equal(item.Status, RegionalHolidayWindowStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RegionalHolidayWindowMovesToGranted()
    {
        var item = RegionalHolidayWindow.NewRegionalHolidayWindow("code-705", "label-811");
        try
        {
            item.MoveTo(RegionalHolidayWindowStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RegionalHolidayWindowStatus.Granted, "status");
    }

    public static void RegionalHolidayWindowRejectsMovesFromExpired()
    {
        var item = RegionalHolidayWindow.NewRegionalHolidayWindow("code-705", "label-811");
        item.Status = RegionalHolidayWindowStatus.Expired;
        Check.Fails(() => item.MoveTo(RegionalHolidayWindowStatus.Requested), "move");
    }

    public static void RunAll()
    {
        RegionalHolidayWindowStartsInRequested();
        RegionalHolidayWindowMovesToGranted();
        RegionalHolidayWindowRejectsMovesFromExpired();
    }
}
