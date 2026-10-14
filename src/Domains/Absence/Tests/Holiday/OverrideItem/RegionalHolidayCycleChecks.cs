namespace Acme.Workforce.Absence.Tests.Holiday.OverrideItem;

using Acme.Workforce.Absence.Holiday.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class RegionalHolidayCycleChecks
{
    public static void RegionalHolidayCycleStartsInRequested()
    {
        var item = RegionalHolidayCycle.NewRegionalHolidayCycle("code-686", "name-130");
        Check.Equal(item.Status, RegionalHolidayCycleStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RegionalHolidayCycleMovesToGranted()
    {
        var item = RegionalHolidayCycle.NewRegionalHolidayCycle("code-686", "name-130");
        try
        {
            item.MoveTo(RegionalHolidayCycleStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RegionalHolidayCycleStatus.Granted, "status");
    }

    public static void RegionalHolidayCycleRejectsMovesFromExpired()
    {
        var item = RegionalHolidayCycle.NewRegionalHolidayCycle("code-686", "name-130");
        item.Status = RegionalHolidayCycleStatus.Expired;
        Check.Fails(() => item.MoveTo(RegionalHolidayCycleStatus.Requested), "move");
    }

    public static void RunAll()
    {
        RegionalHolidayCycleStartsInRequested();
        RegionalHolidayCycleMovesToGranted();
        RegionalHolidayCycleRejectsMovesFromExpired();
    }
}
