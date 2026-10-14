namespace Acme.Workforce.Compensation.Tests.Adjustment.AdjustmentRule;

using Acme.Workforce.Compensation.Adjustment.AdjustmentRule;

/// <summary>Checks of adjustment rule code, run by the project's runner.</summary>
public static class AdjustmentManifestViewChecks
{
    public static void AdjustmentManifestViewShowsTheStatusInCapitals()
    {
        var item = AdjustmentManifest.NewAdjustmentManifest("code-501", "title-865");
        var view = AdjustmentManifestView.ToAdjustmentManifestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AdjustmentManifestView.ToAdjustmentManifestViews(new List<AdjustmentManifest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AdjustmentManifestViewShowsTheStatusInCapitals();
    }
}
