namespace Acme.Workforce.Payrollops.Tests.Payslip.RollingPayslipSnapshot;

using Acme.Workforce.Payrollops.Payslip.RollingPayslipSnapshot;

/// <summary>Checks of rolling payslip snapshot code, run by the project's runner.</summary>
public static class RegionalPayslipExceptionChecks
{
    public static void RegionalPayslipExceptionStartsInDraft()
    {
        var item = RegionalPayslipException.NewRegionalPayslipException("code-463", "label-234");
        Check.Equal(item.Status, RegionalPayslipExceptionStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RegionalPayslipExceptionMovesToActive()
    {
        var item = RegionalPayslipException.NewRegionalPayslipException("code-463", "label-234");
        try
        {
            item.MoveTo(RegionalPayslipExceptionStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RegionalPayslipExceptionStatus.Active, "status");
    }

    public static void RegionalPayslipExceptionRejectsMovesFromClosed()
    {
        var item = RegionalPayslipException.NewRegionalPayslipException("code-463", "label-234");
        item.Status = RegionalPayslipExceptionStatus.Closed;
        Check.Fails(() => item.MoveTo(RegionalPayslipExceptionStatus.Draft), "move");
    }

    public static void RunAll()
    {
        RegionalPayslipExceptionStartsInDraft();
        RegionalPayslipExceptionMovesToActive();
        RegionalPayslipExceptionRejectsMovesFromClosed();
    }
}
