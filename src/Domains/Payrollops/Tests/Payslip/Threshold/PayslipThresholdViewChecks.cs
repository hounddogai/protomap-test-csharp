namespace Acme.Workforce.Payrollops.Tests.Payslip.Threshold;

using Acme.Workforce.Payrollops.Payslip.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class PayslipThresholdViewChecks
{
    public static void PayslipThresholdViewShowsTheStatusInCapitals()
    {
        var item = PayslipThreshold.NewPayslipThreshold("code-409", "label-149");
        var view = PayslipThresholdView.ToPayslipThresholdView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PayslipThresholdView.ToPayslipThresholdViews(new List<PayslipThreshold> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PayslipThresholdViewShowsTheStatusInCapitals();
    }
}
