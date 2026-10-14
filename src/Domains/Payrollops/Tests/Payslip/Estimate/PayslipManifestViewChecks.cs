namespace Acme.Workforce.Payrollops.Tests.Payslip.Estimate;

using Acme.Workforce.Payrollops.Payslip.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class PayslipManifestViewChecks
{
    public static void PayslipManifestViewShowsTheStatusInCapitals()
    {
        var item = PayslipManifest.NewPayslipManifest("code-746", "title-628");
        var view = PayslipManifestView.ToPayslipManifestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PayslipManifestView.ToPayslipManifestViews(new List<PayslipManifest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PayslipManifestViewShowsTheStatusInCapitals();
    }
}
