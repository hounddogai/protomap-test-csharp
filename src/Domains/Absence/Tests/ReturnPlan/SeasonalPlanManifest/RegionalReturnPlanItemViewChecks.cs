namespace Acme.Workforce.Absence.Tests.ReturnPlan.SeasonalPlanManifest;

using Acme.Workforce.Absence.ReturnPlan.SeasonalPlanManifest;

/// <summary>Checks of seasonal plan manifest code, run by the project's runner.</summary>
public static class RegionalReturnPlanItemViewChecks
{
    public static void RegionalReturnPlanItemViewShowsTheStatusInCapitals()
    {
        var item = RegionalReturnPlanItem.NewRegionalReturnPlanItem("code-523", "name-239");
        var view = RegionalReturnPlanItemView.ToRegionalReturnPlanItemView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RegionalReturnPlanItemView.ToRegionalReturnPlanItemViews(new List<RegionalReturnPlanItem> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RegionalReturnPlanItemViewShowsTheStatusInCapitals();
    }
}
