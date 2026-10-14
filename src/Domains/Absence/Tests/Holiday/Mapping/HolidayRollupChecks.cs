namespace Acme.Workforce.Absence.Tests.Holiday.Mapping;

using Acme.Workforce.Absence.Holiday.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class HolidayRollupChecks
{
    public static void HolidayRollupStartsInProposed()
    {
        var item = HolidayRollup.NewHolidayRollup("code-869", "label-564");
        Check.Equal(item.Status, HolidayRollupStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void HolidayRollupMovesToWithdrawn()
    {
        var item = HolidayRollup.NewHolidayRollup("code-869", "label-564");
        try
        {
            item.MoveTo(HolidayRollupStatus.Withdrawn);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, HolidayRollupStatus.Withdrawn, "status");
    }

    public static void HolidayRollupRejectsMovesFromWithdrawn()
    {
        var item = HolidayRollup.NewHolidayRollup("code-869", "label-564");
        item.Status = HolidayRollupStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(HolidayRollupStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        HolidayRollupStartsInProposed();
        HolidayRollupMovesToWithdrawn();
        HolidayRollupRejectsMovesFromWithdrawn();
    }
}
