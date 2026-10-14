namespace Acme.Workforce.Orgdesign.Tests.CostModel.Rule;

using Acme.Workforce.Orgdesign.CostModel.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class LegacyCostModelManifestViewChecks
{
    public static void LegacyCostModelManifestViewShowsTheStatusInCapitals()
    {
        var item = LegacyCostModelManifest.NewLegacyCostModelManifest("code-910", "title-805");
        var view = LegacyCostModelManifestView.ToLegacyCostModelManifestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LegacyCostModelManifestView.ToLegacyCostModelManifestViews(new List<LegacyCostModelManifest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LegacyCostModelManifestViewShowsTheStatusInCapitals();
    }
}
