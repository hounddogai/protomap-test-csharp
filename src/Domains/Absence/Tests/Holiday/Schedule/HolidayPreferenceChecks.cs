namespace Acme.Workforce.Absence.Tests.Holiday.Schedule;

using Acme.Workforce.Absence.Holiday.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class HolidayPreferenceChecks
{
    public static void HolidayPreferenceStartsInIdle()
    {
        var item = HolidayPreference.NewHolidayPreference("code-179", "title-514");
        Check.Equal(item.Status, HolidayPreferenceStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void HolidayPreferenceMovesToReady()
    {
        var item = HolidayPreference.NewHolidayPreference("code-179", "title-514");
        try
        {
            item.MoveTo(HolidayPreferenceStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, HolidayPreferenceStatus.Ready, "status");
    }

    public static void HolidayPreferenceRejectsMovesFromStopped()
    {
        var item = HolidayPreference.NewHolidayPreference("code-179", "title-514");
        item.Status = HolidayPreferenceStatus.Stopped;
        Check.Fails(() => item.MoveTo(HolidayPreferenceStatus.Idle), "move");
    }

    public static void RunAll()
    {
        HolidayPreferenceStartsInIdle();
        HolidayPreferenceMovesToReady();
        HolidayPreferenceRejectsMovesFromStopped();
    }
}
