namespace Acme.Workforce.Assets.Tests.Warranty.Milestone;

using Acme.Workforce.Assets.Warranty.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class SeasonalWarrantyTargetViewChecks
{
    public static void SeasonalWarrantyTargetViewShowsTheStatusInCapitals()
    {
        var item = SeasonalWarrantyTarget.NewSeasonalWarrantyTarget("code-523", "title-632");
        var view = SeasonalWarrantyTargetView.ToSeasonalWarrantyTargetView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SeasonalWarrantyTargetView.ToSeasonalWarrantyTargetViews(new List<SeasonalWarrantyTarget> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SeasonalWarrantyTargetViewShowsTheStatusInCapitals();
    }
}
