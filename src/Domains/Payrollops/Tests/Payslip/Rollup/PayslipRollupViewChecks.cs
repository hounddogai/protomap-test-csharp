namespace Acme.Workforce.Payrollops.Tests.Payslip.Rollup;

using Acme.Workforce.Payrollops.Payslip.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class PayslipRollupViewChecks
{
    public static void PayslipRollupViewShowsTheStatusInCapitals()
    {
        var item = PayslipRollup.NewPayslipRollup("code-501", "name-732");
        var view = PayslipRollupView.ToPayslipRollupView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PayslipRollupView.ToPayslipRollupViews(new List<PayslipRollup> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PayslipRollupViewShowsTheStatusInCapitals();
    }
}
