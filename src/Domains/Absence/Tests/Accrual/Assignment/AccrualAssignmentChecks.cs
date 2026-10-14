namespace Acme.Workforce.Absence.Tests.Accrual.Assignment;

using Acme.Workforce.Absence.Accrual.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class AccrualAssignmentChecks
{
    public static void AccrualAssignmentStartsInProposed()
    {
        var item = AccrualAssignment.NewAccrualAssignment("code-753", "title-491");
        Check.Equal(item.Status, AccrualAssignmentStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AccrualAssignmentMovesToApproved()
    {
        var item = AccrualAssignment.NewAccrualAssignment("code-753", "title-491");
        try
        {
            item.MoveTo(AccrualAssignmentStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AccrualAssignmentStatus.Approved, "status");
    }

    public static void AccrualAssignmentRejectsMovesFromWithdrawn()
    {
        var item = AccrualAssignment.NewAccrualAssignment("code-753", "title-491");
        item.Status = AccrualAssignmentStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(AccrualAssignmentStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        AccrualAssignmentStartsInProposed();
        AccrualAssignmentMovesToApproved();
        AccrualAssignmentRejectsMovesFromWithdrawn();
    }
}
