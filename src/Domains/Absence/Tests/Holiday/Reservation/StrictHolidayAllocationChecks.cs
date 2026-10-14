namespace Acme.Workforce.Absence.Tests.Holiday.Reservation;

using Acme.Workforce.Absence.Holiday.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class StrictHolidayAllocationChecks
{
    public static void StrictHolidayAllocationStartsInProposed()
    {
        var item = StrictHolidayAllocation.NewStrictHolidayAllocation("code-897", "name-527");
        Check.Equal(item.Status, StrictHolidayAllocationStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictHolidayAllocationMovesToApproved()
    {
        var item = StrictHolidayAllocation.NewStrictHolidayAllocation("code-897", "name-527");
        try
        {
            item.MoveTo(StrictHolidayAllocationStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictHolidayAllocationStatus.Approved, "status");
    }

    public static void StrictHolidayAllocationRejectsMovesFromWithdrawn()
    {
        var item = StrictHolidayAllocation.NewStrictHolidayAllocation("code-897", "name-527");
        item.Status = StrictHolidayAllocationStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(StrictHolidayAllocationStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        StrictHolidayAllocationStartsInProposed();
        StrictHolidayAllocationMovesToApproved();
        StrictHolidayAllocationRejectsMovesFromWithdrawn();
    }
}
