namespace Acme.Workforce.Absence.Tests.Holiday.Adjustment;

using Acme.Workforce.Absence.Holiday.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class HolidayAssignmentChecks
{
    public static void HolidayAssignmentStartsInProposed()
    {
        var item = HolidayAssignment.NewHolidayAssignment("code-105", "name-749");
        Check.Equal(item.Status, HolidayAssignmentStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void HolidayAssignmentMovesToApproved()
    {
        var item = HolidayAssignment.NewHolidayAssignment("code-105", "name-749");
        try
        {
            item.MoveTo(HolidayAssignmentStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, HolidayAssignmentStatus.Approved, "status");
    }

    public static void HolidayAssignmentRejectsMovesFromWithdrawn()
    {
        var item = HolidayAssignment.NewHolidayAssignment("code-105", "name-749");
        item.Status = HolidayAssignmentStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(HolidayAssignmentStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        HolidayAssignmentStartsInProposed();
        HolidayAssignmentMovesToApproved();
        HolidayAssignmentRejectsMovesFromWithdrawn();
    }
}
