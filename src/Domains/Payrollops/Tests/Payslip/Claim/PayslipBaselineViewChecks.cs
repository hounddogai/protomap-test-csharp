namespace Acme.Workforce.Payrollops.Tests.Payslip.Claim;

using Acme.Workforce.Payrollops.Payslip.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class PayslipBaselineViewChecks
{
    public static void PayslipBaselineViewShowsTheStatusInCapitals()
    {
        var item = PayslipBaseline.NewPayslipBaseline("code-7", "title-931");
        var view = PayslipBaselineView.ToPayslipBaselineView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PayslipBaselineView.ToPayslipBaselineViews(new List<PayslipBaseline> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PayslipBaselineViewShowsTheStatusInCapitals();
    }
}
