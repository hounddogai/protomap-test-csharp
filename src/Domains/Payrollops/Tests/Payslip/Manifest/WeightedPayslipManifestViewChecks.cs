namespace Acme.Workforce.Payrollops.Tests.Payslip.Manifest;

using Acme.Workforce.Payrollops.Payslip.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class WeightedPayslipManifestViewChecks
{
    public static void WeightedPayslipManifestViewShowsTheStatusInCapitals()
    {
        var item = WeightedPayslipManifest.NewWeightedPayslipManifest("code-471", "name-886");
        var view = WeightedPayslipManifestView.ToWeightedPayslipManifestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(WeightedPayslipManifestView.ToWeightedPayslipManifestViews(new List<WeightedPayslipManifest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        WeightedPayslipManifestViewShowsTheStatusInCapitals();
    }
}
