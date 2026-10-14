namespace Acme.Workforce.Payrollops.Tests.Deduction.Assignment;

using Acme.Workforce.Payrollops.Deduction.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class TieredDeductionManifestViewChecks
{
    public static void TieredDeductionManifestViewShowsTheStatusInCapitals()
    {
        var item = TieredDeductionManifest.NewTieredDeductionManifest("code-614", "label-816");
        var view = TieredDeductionManifestView.ToTieredDeductionManifestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TieredDeductionManifestView.ToTieredDeductionManifestViews(new List<TieredDeductionManifest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TieredDeductionManifestViewShowsTheStatusInCapitals();
    }
}
