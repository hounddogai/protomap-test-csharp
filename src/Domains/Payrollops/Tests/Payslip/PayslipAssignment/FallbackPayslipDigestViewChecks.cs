namespace Acme.Workforce.Payrollops.Tests.Payslip.PayslipAssignment;

using Acme.Workforce.Payrollops.Payslip.PayslipAssignment;

/// <summary>Checks of payslip assignment code, run by the project's runner.</summary>
public static class FallbackPayslipDigestViewChecks
{
    public static void FallbackPayslipDigestViewShowsTheStatusInCapitals()
    {
        var item = FallbackPayslipDigest.NewFallbackPayslipDigest("code-804", "name-775");
        var view = FallbackPayslipDigestView.ToFallbackPayslipDigestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(FallbackPayslipDigestView.ToFallbackPayslipDigestViews(new List<FallbackPayslipDigest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        FallbackPayslipDigestViewShowsTheStatusInCapitals();
    }
}
