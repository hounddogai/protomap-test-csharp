namespace Acme.Workforce.Absence.Tests.Holiday.HolidayPolicy;

using Acme.Workforce.Absence.Holiday.HolidayPolicy;

/// <summary>Checks of holiday policy code, run by the project's runner.</summary>
public static class RegionalHolidayPolicyChecks
{
    public static void RegionalHolidayPolicyStartsInPlanned()
    {
        var item = RegionalHolidayPolicy.NewRegionalHolidayPolicy("code-585", "name-273");
        Check.Equal(item.Status, RegionalHolidayPolicyStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RegionalHolidayPolicyMovesToInProgress()
    {
        var item = RegionalHolidayPolicy.NewRegionalHolidayPolicy("code-585", "name-273");
        try
        {
            item.MoveTo(RegionalHolidayPolicyStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RegionalHolidayPolicyStatus.InProgress, "status");
    }

    public static void RegionalHolidayPolicyRejectsMovesFromCancelled()
    {
        var item = RegionalHolidayPolicy.NewRegionalHolidayPolicy("code-585", "name-273");
        item.Status = RegionalHolidayPolicyStatus.Cancelled;
        Check.Fails(() => item.MoveTo(RegionalHolidayPolicyStatus.Planned), "move");
    }

    public static void RunAll()
    {
        RegionalHolidayPolicyStartsInPlanned();
        RegionalHolidayPolicyMovesToInProgress();
        RegionalHolidayPolicyRejectsMovesFromCancelled();
    }
}
