namespace Acme.Workforce.Payrollops.Tests.Payslip.PayslipNotice;

using Acme.Workforce.Payrollops.Payslip.PayslipNotice;

/// <summary>Checks of payslip notice code, run by the project's runner.</summary>
public static class BatchedPayslipClaimViewChecks
{
    public static void BatchedPayslipClaimViewShowsTheStatusInCapitals()
    {
        var item = BatchedPayslipClaim.NewBatchedPayslipClaim("code-857", "name-550");
        var view = BatchedPayslipClaimView.ToBatchedPayslipClaimView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BatchedPayslipClaimView.ToBatchedPayslipClaimViews(new List<BatchedPayslipClaim> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BatchedPayslipClaimViewShowsTheStatusInCapitals();
    }
}
