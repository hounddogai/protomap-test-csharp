namespace Acme.Workforce.Payrollops.Tests.Payslip.PayslipAssignment;

using Acme.Workforce.Payrollops.Payslip.PayslipAssignment;

/// <summary>Checks of payslip assignment code, run by the project's runner.</summary>
public static class RollingPayslipAssignmentChecks
{
    public static void RollingPayslipAssignmentStartsInOpen()
    {
        var item = RollingPayslipAssignment.NewRollingPayslipAssignment("code-222", "label-856");
        Check.Equal(item.Status, RollingPayslipAssignmentStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RollingPayslipAssignmentMovesToResolved()
    {
        var item = RollingPayslipAssignment.NewRollingPayslipAssignment("code-222", "label-856");
        try
        {
            item.MoveTo(RollingPayslipAssignmentStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RollingPayslipAssignmentStatus.Resolved, "status");
    }

    public static void RollingPayslipAssignmentRejectsMovesFromArchived()
    {
        var item = RollingPayslipAssignment.NewRollingPayslipAssignment("code-222", "label-856");
        item.Status = RollingPayslipAssignmentStatus.Archived;
        Check.Fails(() => item.MoveTo(RollingPayslipAssignmentStatus.Open), "move");
    }

    public static void RunAll()
    {
        RollingPayslipAssignmentStartsInOpen();
        RollingPayslipAssignmentMovesToResolved();
        RollingPayslipAssignmentRejectsMovesFromArchived();
    }
}
