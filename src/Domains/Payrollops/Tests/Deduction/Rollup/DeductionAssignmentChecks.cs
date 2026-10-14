namespace Acme.Workforce.Payrollops.Tests.Deduction.Rollup;

using Acme.Workforce.Payrollops.Deduction.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class DeductionAssignmentChecks
{
    public static void DeductionAssignmentStartsInProposed()
    {
        var item = DeductionAssignment.NewDeductionAssignment("code-887", "title-87");
        Check.Equal(item.Status, DeductionAssignmentStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DeductionAssignmentMovesToApproved()
    {
        var item = DeductionAssignment.NewDeductionAssignment("code-887", "title-87");
        try
        {
            item.MoveTo(DeductionAssignmentStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DeductionAssignmentStatus.Approved, "status");
    }

    public static void DeductionAssignmentRejectsMovesFromWithdrawn()
    {
        var item = DeductionAssignment.NewDeductionAssignment("code-887", "title-87");
        item.Status = DeductionAssignmentStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(DeductionAssignmentStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        DeductionAssignmentStartsInProposed();
        DeductionAssignmentMovesToApproved();
        DeductionAssignmentRejectsMovesFromWithdrawn();
    }
}
