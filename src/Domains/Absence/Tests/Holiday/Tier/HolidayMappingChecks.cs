namespace Acme.Workforce.Absence.Tests.Holiday.Tier;

using Acme.Workforce.Absence.Holiday.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class HolidayMappingChecks
{
    public static void HolidayMappingStartsInCreated()
    {
        var item = HolidayMapping.NewHolidayMapping("code-726", "label-103");
        Check.Equal(item.Status, HolidayMappingStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void HolidayMappingMovesToRetired()
    {
        var item = HolidayMapping.NewHolidayMapping("code-726", "label-103");
        try
        {
            item.MoveTo(HolidayMappingStatus.Retired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, HolidayMappingStatus.Retired, "status");
    }

    public static void HolidayMappingRejectsMovesFromRetired()
    {
        var item = HolidayMapping.NewHolidayMapping("code-726", "label-103");
        item.Status = HolidayMappingStatus.Retired;
        Check.Fails(() => item.MoveTo(HolidayMappingStatus.Created), "move");
    }

    public static void RunAll()
    {
        HolidayMappingStartsInCreated();
        HolidayMappingMovesToRetired();
        HolidayMappingRejectsMovesFromRetired();
    }
}
