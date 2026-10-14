namespace Acme.Workforce.Payrollops.Tests.Payslip.PayslipDigest;

using Acme.Workforce.Payrollops.Payslip.PayslipDigest;

/// <summary>Checks of payslip digest code, run by the project's runner.</summary>
public static class LegacyPayslipDigestViewChecks
{
    public static void LegacyPayslipDigestViewShowsTheStatusInCapitals()
    {
        var item = LegacyPayslipDigest.NewLegacyPayslipDigest("code-876", "title-810");
        var view = LegacyPayslipDigestView.ToLegacyPayslipDigestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LegacyPayslipDigestView.ToLegacyPayslipDigestViews(new List<LegacyPayslipDigest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LegacyPayslipDigestViewShowsTheStatusInCapitals();
    }
}
