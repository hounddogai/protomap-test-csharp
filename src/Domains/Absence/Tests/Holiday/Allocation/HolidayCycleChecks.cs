namespace Acme.Workforce.Absence.Tests.Holiday.Allocation;

using Acme.Workforce.Absence.Holiday.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class HolidayCycleChecks
{
    public static void HolidayCycleStartsInCreated()
    {
        var item = HolidayCycle.NewHolidayCycle("code-555", "title-397");
        Check.Equal(item.Status, HolidayCycleStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void HolidayCycleMovesToValidated()
    {
        var item = HolidayCycle.NewHolidayCycle("code-555", "title-397");
        try
        {
            item.MoveTo(HolidayCycleStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, HolidayCycleStatus.Validated, "status");
    }

    public static void HolidayCycleRejectsMovesFromRetired()
    {
        var item = HolidayCycle.NewHolidayCycle("code-555", "title-397");
        item.Status = HolidayCycleStatus.Retired;
        Check.Fails(() => item.MoveTo(HolidayCycleStatus.Created), "move");
    }

    public static void RunAll()
    {
        HolidayCycleStartsInCreated();
        HolidayCycleMovesToValidated();
        HolidayCycleRejectsMovesFromRetired();
    }
}
