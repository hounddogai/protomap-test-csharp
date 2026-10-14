namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Ledger;

using Acme.Workforce.Talent.SuccessionPlan.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class SuccessionPlanManifestViewChecks
{
    public static void SuccessionPlanManifestViewShowsTheStatusInCapitals()
    {
        var item = SuccessionPlanManifest.NewSuccessionPlanManifest("code-899", "label-794");
        var view = SuccessionPlanManifestView.ToSuccessionPlanManifestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SuccessionPlanManifestView.ToSuccessionPlanManifestViews(new List<SuccessionPlanManifest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SuccessionPlanManifestViewShowsTheStatusInCapitals();
    }
}
