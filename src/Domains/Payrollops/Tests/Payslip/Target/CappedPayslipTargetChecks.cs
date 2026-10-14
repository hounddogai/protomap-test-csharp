namespace Acme.Workforce.Payrollops.Tests.Payslip.Target;

using Acme.Workforce.Payrollops.Payslip.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class CappedPayslipTargetChecks
{
    public static void CappedPayslipTargetStartsInPlanned()
    {
        var item = CappedPayslipTarget.NewCappedPayslipTarget("code-787", "label-658");
        Check.Equal(item.Status, CappedPayslipTargetStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedPayslipTargetMovesToScheduled()
    {
        var item = CappedPayslipTarget.NewCappedPayslipTarget("code-787", "label-658");
        try
        {
            item.MoveTo(CappedPayslipTargetStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedPayslipTargetStatus.Scheduled, "status");
    }

    public static void CappedPayslipTargetRejectsMovesFromCancelled()
    {
        var item = CappedPayslipTarget.NewCappedPayslipTarget("code-787", "label-658");
        item.Status = CappedPayslipTargetStatus.Cancelled;
        Check.Fails(() => item.MoveTo(CappedPayslipTargetStatus.Planned), "move");
    }

    public static void RunAll()
    {
        CappedPayslipTargetStartsInPlanned();
        CappedPayslipTargetMovesToScheduled();
        CappedPayslipTargetRejectsMovesFromCancelled();
    }
}
