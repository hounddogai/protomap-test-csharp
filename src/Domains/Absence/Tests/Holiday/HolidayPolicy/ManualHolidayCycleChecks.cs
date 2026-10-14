namespace Acme.Workforce.Absence.Tests.Holiday.HolidayPolicy;

using Acme.Workforce.Absence.Holiday.HolidayPolicy;

/// <summary>Checks of holiday policy code, run by the project's runner.</summary>
public static class ManualHolidayCycleChecks
{
    public static void ManualHolidayCycleStartsInNew()
    {
        var item = ManualHolidayCycle.NewManualHolidayCycle("code-972", "label-934");
        Check.Equal(item.Status, ManualHolidayCycleStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ManualHolidayCycleMovesToRunning()
    {
        var item = ManualHolidayCycle.NewManualHolidayCycle("code-972", "label-934");
        try
        {
            item.MoveTo(ManualHolidayCycleStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ManualHolidayCycleStatus.Running, "status");
    }

    public static void ManualHolidayCycleRejectsMovesFromFailed()
    {
        var item = ManualHolidayCycle.NewManualHolidayCycle("code-972", "label-934");
        item.Status = ManualHolidayCycleStatus.Failed;
        Check.Fails(() => item.MoveTo(ManualHolidayCycleStatus.New), "move");
    }

    public static void RunAll()
    {
        ManualHolidayCycleStartsInNew();
        ManualHolidayCycleMovesToRunning();
        ManualHolidayCycleRejectsMovesFromFailed();
    }
}
