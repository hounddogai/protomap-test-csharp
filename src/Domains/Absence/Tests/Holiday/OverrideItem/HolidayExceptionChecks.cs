namespace Acme.Workforce.Absence.Tests.Holiday.OverrideItem;

using Acme.Workforce.Absence.Holiday.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class HolidayExceptionChecks
{
    public static void HolidayExceptionStartsInIdle()
    {
        var item = HolidayException.NewHolidayException("code-672", "title-760");
        Check.Equal(item.Status, HolidayExceptionStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void HolidayExceptionMovesToWarming()
    {
        var item = HolidayException.NewHolidayException("code-672", "title-760");
        try
        {
            item.MoveTo(HolidayExceptionStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, HolidayExceptionStatus.Warming, "status");
    }

    public static void HolidayExceptionRejectsMovesFromStopped()
    {
        var item = HolidayException.NewHolidayException("code-672", "title-760");
        item.Status = HolidayExceptionStatus.Stopped;
        Check.Fails(() => item.MoveTo(HolidayExceptionStatus.Idle), "move");
    }

    public static void RunAll()
    {
        HolidayExceptionStartsInIdle();
        HolidayExceptionMovesToWarming();
        HolidayExceptionRejectsMovesFromStopped();
    }
}
