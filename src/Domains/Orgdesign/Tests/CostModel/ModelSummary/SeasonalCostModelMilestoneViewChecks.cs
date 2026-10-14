namespace Acme.Workforce.Orgdesign.Tests.CostModel.ModelSummary;

using Acme.Workforce.Orgdesign.CostModel.ModelSummary;

/// <summary>Checks of model summary code, run by the project's runner.</summary>
public static class SeasonalCostModelMilestoneViewChecks
{
    public static void SeasonalCostModelMilestoneViewShowsTheStatusInCapitals()
    {
        var item = SeasonalCostModelMilestone.NewSeasonalCostModelMilestone("code-488", "label-772");
        var view = SeasonalCostModelMilestoneView.ToSeasonalCostModelMilestoneView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SeasonalCostModelMilestoneView.ToSeasonalCostModelMilestoneViews(new List<SeasonalCostModelMilestone> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SeasonalCostModelMilestoneViewShowsTheStatusInCapitals();
    }
}
