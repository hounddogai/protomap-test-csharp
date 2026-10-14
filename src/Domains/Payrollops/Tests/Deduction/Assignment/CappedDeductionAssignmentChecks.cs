namespace Acme.Workforce.Payrollops.Tests.Deduction.Assignment;

using Acme.Workforce.Payrollops.Deduction.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class CappedDeductionAssignmentChecks
{
    public static void CappedDeductionAssignmentStartsInOpen()
    {
        var item = CappedDeductionAssignment.NewCappedDeductionAssignment("code-587", "label-243");
        Check.Equal(item.Status, CappedDeductionAssignmentStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedDeductionAssignmentMovesToArchived()
    {
        var item = CappedDeductionAssignment.NewCappedDeductionAssignment("code-587", "label-243");
        try
        {
            item.MoveTo(CappedDeductionAssignmentStatus.Archived);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedDeductionAssignmentStatus.Archived, "status");
    }

    public static void CappedDeductionAssignmentRejectsMovesFromArchived()
    {
        var item = CappedDeductionAssignment.NewCappedDeductionAssignment("code-587", "label-243");
        item.Status = CappedDeductionAssignmentStatus.Archived;
        Check.Fails(() => item.MoveTo(CappedDeductionAssignmentStatus.Open), "move");
    }

    public static void RunAll()
    {
        CappedDeductionAssignmentStartsInOpen();
        CappedDeductionAssignmentMovesToArchived();
        CappedDeductionAssignmentRejectsMovesFromArchived();
    }
}
