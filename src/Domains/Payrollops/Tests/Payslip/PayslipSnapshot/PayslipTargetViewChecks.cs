namespace Acme.Workforce.Payrollops.Tests.Payslip.PayslipSnapshot;

using Acme.Workforce.Payrollops.Payslip.PayslipSnapshot;

/// <summary>Checks of payslip snapshot code, run by the project's runner.</summary>
public static class PayslipTargetViewChecks
{
    public static void PayslipTargetViewShowsTheStatusInCapitals()
    {
        var item = PayslipTarget.NewPayslipTarget("code-168", "label-501");
        var view = PayslipTargetView.ToPayslipTargetView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PayslipTargetView.ToPayslipTargetViews(new List<PayslipTarget> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PayslipTargetViewShowsTheStatusInCapitals();
    }
}
