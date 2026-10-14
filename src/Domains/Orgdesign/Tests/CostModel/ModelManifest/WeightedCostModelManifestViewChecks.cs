namespace Acme.Workforce.Orgdesign.Tests.CostModel.ModelManifest;

using Acme.Workforce.Orgdesign.CostModel.ModelManifest;

/// <summary>Checks of model manifest code, run by the project's runner.</summary>
public static class WeightedCostModelManifestViewChecks
{
    public static void WeightedCostModelManifestViewShowsTheStatusInCapitals()
    {
        var item = WeightedCostModelManifest.NewWeightedCostModelManifest("code-284", "name-702");
        var view = WeightedCostModelManifestView.ToWeightedCostModelManifestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(WeightedCostModelManifestView.ToWeightedCostModelManifestViews(new List<WeightedCostModelManifest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        WeightedCostModelManifestViewShowsTheStatusInCapitals();
    }
}
