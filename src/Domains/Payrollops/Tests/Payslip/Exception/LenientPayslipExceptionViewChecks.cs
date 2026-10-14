namespace Acme.Workforce.Payrollops.Tests.Payslip.Exception;

using Acme.Workforce.Payrollops.Payslip.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class LenientPayslipExceptionViewChecks
{
    public static void LenientPayslipExceptionViewShowsTheStatusInCapitals()
    {
        var item = LenientPayslipException.NewLenientPayslipException("code-136", "label-704");
        var view = LenientPayslipExceptionView.ToLenientPayslipExceptionView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LenientPayslipExceptionView.ToLenientPayslipExceptionViews(new List<LenientPayslipException> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LenientPayslipExceptionViewShowsTheStatusInCapitals();
    }
}
