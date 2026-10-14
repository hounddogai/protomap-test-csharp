namespace Acme.Workforce.Absence.Tests.Holiday.Schedule;

using Acme.Workforce.Absence.Holiday.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class ManualHolidayVarianceChecks
{
    public static void ManualHolidayVarianceStartsInPlanned()
    {
        var item = ManualHolidayVariance.NewManualHolidayVariance("code-884", "name-510");
        Check.Equal(item.Status, ManualHolidayVarianceStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ManualHolidayVarianceMovesToScheduled()
    {
        var item = ManualHolidayVariance.NewManualHolidayVariance("code-884", "name-510");
        try
        {
            item.MoveTo(ManualHolidayVarianceStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ManualHolidayVarianceStatus.Scheduled, "status");
    }

    public static void ManualHolidayVarianceRejectsMovesFromCancelled()
    {
        var item = ManualHolidayVariance.NewManualHolidayVariance("code-884", "name-510");
        item.Status = ManualHolidayVarianceStatus.Cancelled;
        Check.Fails(() => item.MoveTo(ManualHolidayVarianceStatus.Planned), "move");
    }

    public static void RunAll()
    {
        ManualHolidayVarianceStartsInPlanned();
        ManualHolidayVarianceMovesToScheduled();
        ManualHolidayVarianceRejectsMovesFromCancelled();
    }
}
