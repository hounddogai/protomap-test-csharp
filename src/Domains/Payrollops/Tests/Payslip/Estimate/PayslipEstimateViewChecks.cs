namespace Acme.Workforce.Payrollops.Tests.Payslip.Estimate;

using Acme.Workforce.Payrollops.Payslip.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class PayslipEstimateViewChecks
{
    public static void PayslipEstimateViewShowsTheStatusInCapitals()
    {
        var item = PayslipEstimate.NewPayslipEstimate("code-660", "name-552");
        var view = PayslipEstimateView.ToPayslipEstimateView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PayslipEstimateView.ToPayslipEstimateViews(new List<PayslipEstimate> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PayslipEstimateViewShowsTheStatusInCapitals();
    }
}
