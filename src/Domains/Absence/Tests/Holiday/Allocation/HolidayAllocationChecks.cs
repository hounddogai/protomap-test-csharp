namespace Acme.Workforce.Absence.Tests.Holiday.Allocation;

using Acme.Workforce.Absence.Holiday.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class HolidayAllocationChecks
{
    public static void HolidayAllocationStartsInPlanned()
    {
        var item = HolidayAllocation.NewHolidayAllocation("code-320", "name-777");
        Check.Equal(item.Status, HolidayAllocationStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void HolidayAllocationMovesToScheduled()
    {
        var item = HolidayAllocation.NewHolidayAllocation("code-320", "name-777");
        try
        {
            item.MoveTo(HolidayAllocationStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, HolidayAllocationStatus.Scheduled, "status");
    }

    public static void HolidayAllocationRejectsMovesFromCancelled()
    {
        var item = HolidayAllocation.NewHolidayAllocation("code-320", "name-777");
        item.Status = HolidayAllocationStatus.Cancelled;
        Check.Fails(() => item.MoveTo(HolidayAllocationStatus.Planned), "move");
    }

    public static void RunAll()
    {
        HolidayAllocationStartsInPlanned();
        HolidayAllocationMovesToScheduled();
        HolidayAllocationRejectsMovesFromCancelled();
    }
}
