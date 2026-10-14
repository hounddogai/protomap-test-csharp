namespace Acme.Workforce.Absence.Tests.Accrual.PendingAccrualAssignment;

using Acme.Workforce.Absence.Accrual.PendingAccrualAssignment;

/// <summary>Checks of pending accrual assignment code, run by the project's runner.</summary>
public static class RollingAccrualAssignmentChecks
{
    public static void RollingAccrualAssignmentStartsInProposed()
    {
        var item = RollingAccrualAssignment.NewRollingAccrualAssignment("code-491", "title-549");
        Check.Equal(item.Status, RollingAccrualAssignmentStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RollingAccrualAssignmentMovesToApproved()
    {
        var item = RollingAccrualAssignment.NewRollingAccrualAssignment("code-491", "title-549");
        try
        {
            item.MoveTo(RollingAccrualAssignmentStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RollingAccrualAssignmentStatus.Approved, "status");
    }

    public static void RollingAccrualAssignmentRejectsMovesFromWithdrawn()
    {
        var item = RollingAccrualAssignment.NewRollingAccrualAssignment("code-491", "title-549");
        item.Status = RollingAccrualAssignmentStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(RollingAccrualAssignmentStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        RollingAccrualAssignmentStartsInProposed();
        RollingAccrualAssignmentMovesToApproved();
        RollingAccrualAssignmentRejectsMovesFromWithdrawn();
    }
}
