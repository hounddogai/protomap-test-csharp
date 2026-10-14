namespace Acme.Workforce.Absence.Tests.ReturnPlan.Agreement;

using Acme.Workforce.Absence.ReturnPlan.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class WeightedReturnPlanManifestViewChecks
{
    public static void WeightedReturnPlanManifestViewShowsTheStatusInCapitals()
    {
        var item = WeightedReturnPlanManifest.NewWeightedReturnPlanManifest("code-991", "name-862");
        var view = WeightedReturnPlanManifestView.ToWeightedReturnPlanManifestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(WeightedReturnPlanManifestView.ToWeightedReturnPlanManifestViews(new List<WeightedReturnPlanManifest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        WeightedReturnPlanManifestViewShowsTheStatusInCapitals();
    }
}
