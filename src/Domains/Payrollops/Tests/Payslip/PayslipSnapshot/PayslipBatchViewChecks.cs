namespace Acme.Workforce.Payrollops.Tests.Payslip.PayslipSnapshot;

using Acme.Workforce.Payrollops.Payslip.PayslipSnapshot;

/// <summary>Checks of payslip snapshot code, run by the project's runner.</summary>
public static class PayslipBatchViewChecks
{
    public static void PayslipBatchViewShowsTheStatusInCapitals()
    {
        var item = PayslipBatch.NewPayslipBatch("code-142", "title-759");
        var view = PayslipBatchView.ToPayslipBatchView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PayslipBatchView.ToPayslipBatchViews(new List<PayslipBatch> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PayslipBatchViewShowsTheStatusInCapitals();
    }
}
