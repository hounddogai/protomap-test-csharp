namespace Acme.Workforce.Absence.Tests.Holiday.HolidayReservation;

using Acme.Workforce.Absence.Holiday.HolidayReservation;

/// <summary>Checks of holiday reservation code, run by the project's runner.</summary>
public static class WeightedHolidayMappingChecks
{
    public static void WeightedHolidayMappingStartsInIdle()
    {
        var item = WeightedHolidayMapping.NewWeightedHolidayMapping("code-565", "name-199");
        Check.Equal(item.Status, WeightedHolidayMappingStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WeightedHolidayMappingMovesToWarming()
    {
        var item = WeightedHolidayMapping.NewWeightedHolidayMapping("code-565", "name-199");
        try
        {
            item.MoveTo(WeightedHolidayMappingStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WeightedHolidayMappingStatus.Warming, "status");
    }

    public static void WeightedHolidayMappingRejectsMovesFromStopped()
    {
        var item = WeightedHolidayMapping.NewWeightedHolidayMapping("code-565", "name-199");
        item.Status = WeightedHolidayMappingStatus.Stopped;
        Check.Fails(() => item.MoveTo(WeightedHolidayMappingStatus.Idle), "move");
    }

    public static void RunAll()
    {
        WeightedHolidayMappingStartsInIdle();
        WeightedHolidayMappingMovesToWarming();
        WeightedHolidayMappingRejectsMovesFromStopped();
    }
}
