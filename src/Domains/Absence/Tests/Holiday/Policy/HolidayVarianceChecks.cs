namespace Acme.Workforce.Absence.Tests.Holiday.Policy;

using Acme.Workforce.Absence.Holiday.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class HolidayVarianceChecks
{
    public static void HolidayVarianceStartsInPlanned()
    {
        var item = HolidayVariance.NewHolidayVariance("code-624", "name-300");
        Check.Equal(item.Status, HolidayVarianceStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void HolidayVarianceMovesToCancelled()
    {
        var item = HolidayVariance.NewHolidayVariance("code-624", "name-300");
        try
        {
            item.MoveTo(HolidayVarianceStatus.Cancelled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, HolidayVarianceStatus.Cancelled, "status");
    }

    public static void HolidayVarianceRejectsMovesFromCancelled()
    {
        var item = HolidayVariance.NewHolidayVariance("code-624", "name-300");
        item.Status = HolidayVarianceStatus.Cancelled;
        Check.Fails(() => item.MoveTo(HolidayVarianceStatus.Planned), "move");
    }

    public static void RunAll()
    {
        HolidayVarianceStartsInPlanned();
        HolidayVarianceMovesToCancelled();
        HolidayVarianceRejectsMovesFromCancelled();
    }
}
