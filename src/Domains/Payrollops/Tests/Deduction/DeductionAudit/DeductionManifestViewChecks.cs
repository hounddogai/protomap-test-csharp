namespace Acme.Workforce.Payrollops.Tests.Deduction.DeductionAudit;

using Acme.Workforce.Payrollops.Deduction.DeductionAudit;

/// <summary>Checks of deduction audit code, run by the project's runner.</summary>
public static class DeductionManifestViewChecks
{
    public static void DeductionManifestViewShowsTheStatusInCapitals()
    {
        var item = DeductionManifest.NewDeductionManifest("code-561", "name-54");
        var view = DeductionManifestView.ToDeductionManifestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DeductionManifestView.ToDeductionManifestViews(new List<DeductionManifest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DeductionManifestViewShowsTheStatusInCapitals();
    }
}
