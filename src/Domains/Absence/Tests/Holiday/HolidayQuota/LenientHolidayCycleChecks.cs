namespace Acme.Workforce.Absence.Tests.Holiday.HolidayQuota;

using Acme.Workforce.Absence.Holiday.HolidayQuota;

/// <summary>Checks of holiday quota code, run by the project's runner.</summary>
public static class LenientHolidayCycleChecks
{
    public static void LenientHolidayCycleStartsInCreated()
    {
        var item = LenientHolidayCycle.NewLenientHolidayCycle("code-479", "name-598");
        Check.Equal(item.Status, LenientHolidayCycleStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientHolidayCycleMovesToValidated()
    {
        var item = LenientHolidayCycle.NewLenientHolidayCycle("code-479", "name-598");
        try
        {
            item.MoveTo(LenientHolidayCycleStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientHolidayCycleStatus.Validated, "status");
    }

    public static void LenientHolidayCycleRejectsMovesFromRetired()
    {
        var item = LenientHolidayCycle.NewLenientHolidayCycle("code-479", "name-598");
        item.Status = LenientHolidayCycleStatus.Retired;
        Check.Fails(() => item.MoveTo(LenientHolidayCycleStatus.Created), "move");
    }

    public static void RunAll()
    {
        LenientHolidayCycleStartsInCreated();
        LenientHolidayCycleMovesToValidated();
        LenientHolidayCycleRejectsMovesFromRetired();
    }
}
