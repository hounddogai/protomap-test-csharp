namespace Acme.Workforce.Payrollops.Tests.Payslip.Batch;

using Acme.Workforce.Payrollops.Payslip.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class PayslipAdjustmentViewChecks
{
    public static void PayslipAdjustmentViewShowsTheStatusInCapitals()
    {
        var item = PayslipAdjustment.NewPayslipAdjustment("code-920", "name-767");
        var view = PayslipAdjustmentView.ToPayslipAdjustmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PayslipAdjustmentView.ToPayslipAdjustmentViews(new List<PayslipAdjustment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PayslipAdjustmentViewShowsTheStatusInCapitals();
    }
}
