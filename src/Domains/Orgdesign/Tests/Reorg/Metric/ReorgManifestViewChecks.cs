namespace Acme.Workforce.Orgdesign.Tests.Reorg.Metric;

using Acme.Workforce.Orgdesign.Reorg.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class ReorgManifestViewChecks
{
    public static void ReorgManifestViewShowsTheStatusInCapitals()
    {
        var item = ReorgManifest.NewReorgManifest("code-311", "title-945");
        var view = ReorgManifestView.ToReorgManifestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ReorgManifestView.ToReorgManifestViews(new List<ReorgManifest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ReorgManifestViewShowsTheStatusInCapitals();
    }
}
