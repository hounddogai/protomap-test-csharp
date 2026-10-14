namespace Acme.Workforce.Payrollops.Tests.Payslip.Exception;

using Acme.Workforce.Payrollops.Payslip.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class LenientPayslipExceptionChecks
{
    public static void LenientPayslipExceptionStartsInIdle()
    {
        var item = LenientPayslipException.NewLenientPayslipException("code-136", "label-704");
        Check.Equal(item.Status, LenientPayslipExceptionStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientPayslipExceptionMovesToWarming()
    {
        var item = LenientPayslipException.NewLenientPayslipException("code-136", "label-704");
        try
        {
            item.MoveTo(LenientPayslipExceptionStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientPayslipExceptionStatus.Warming, "status");
    }

    public static void LenientPayslipExceptionRejectsMovesFromStopped()
    {
        var item = LenientPayslipException.NewLenientPayslipException("code-136", "label-704");
        item.Status = LenientPayslipExceptionStatus.Stopped;
        Check.Fails(() => item.MoveTo(LenientPayslipExceptionStatus.Idle), "move");
    }

    public static void RunAll()
    {
        LenientPayslipExceptionStartsInIdle();
        LenientPayslipExceptionMovesToWarming();
        LenientPayslipExceptionRejectsMovesFromStopped();
    }
}
