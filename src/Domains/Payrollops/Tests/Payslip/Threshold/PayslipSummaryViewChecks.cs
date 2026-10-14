namespace Acme.Workforce.Payrollops.Tests.Payslip.Threshold;

using Acme.Workforce.Payrollops.Payslip.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class PayslipSummaryViewChecks
{
    public static void PayslipSummaryViewShowsTheStatusInCapitals()
    {
        var item = PayslipSummary.NewPayslipSummary("code-585", "name-840");
        var view = PayslipSummaryView.ToPayslipSummaryView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PayslipSummaryView.ToPayslipSummaryViews(new List<PayslipSummary> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PayslipSummaryViewShowsTheStatusInCapitals();
    }
}
