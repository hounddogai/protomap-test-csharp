namespace Acme.Workforce.Payrollops.Tests.Payslip.PayslipAssignment;

using Acme.Workforce.Payrollops.Payslip.PayslipAssignment;

/// <summary>Checks of payslip assignment code, run by the project's runner.</summary>
public static class LenientPayslipRollupViewChecks
{
    public static void LenientPayslipRollupViewShowsTheStatusInCapitals()
    {
        var item = LenientPayslipRollup.NewLenientPayslipRollup("code-887", "title-377");
        var view = LenientPayslipRollupView.ToLenientPayslipRollupView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LenientPayslipRollupView.ToLenientPayslipRollupViews(new List<LenientPayslipRollup> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LenientPayslipRollupViewShowsTheStatusInCapitals();
    }
}
