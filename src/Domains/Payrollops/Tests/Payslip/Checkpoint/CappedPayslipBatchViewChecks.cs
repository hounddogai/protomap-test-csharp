namespace Acme.Workforce.Payrollops.Tests.Payslip.Checkpoint;

using Acme.Workforce.Payrollops.Payslip.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class CappedPayslipBatchViewChecks
{
    public static void CappedPayslipBatchViewShowsTheStatusInCapitals()
    {
        var item = CappedPayslipBatch.NewCappedPayslipBatch("code-23", "label-81");
        var view = CappedPayslipBatchView.ToCappedPayslipBatchView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CappedPayslipBatchView.ToCappedPayslipBatchViews(new List<CappedPayslipBatch> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CappedPayslipBatchViewShowsTheStatusInCapitals();
    }
}
