namespace Acme.Workforce.Payrollops.Tests.Garnishment.DefaultGarnishmentAllocation;

using Acme.Workforce.Payrollops.Garnishment.DefaultGarnishmentAllocation;

/// <summary>Checks of default garnishment allocation code, run by the project's runner.</summary>
public static class WeightedGarnishmentMilestoneViewChecks
{
    public static void WeightedGarnishmentMilestoneViewShowsTheStatusInCapitals()
    {
        var item = WeightedGarnishmentMilestone.NewWeightedGarnishmentMilestone("code-474", "title-665");
        var view = WeightedGarnishmentMilestoneView.ToWeightedGarnishmentMilestoneView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(WeightedGarnishmentMilestoneView.ToWeightedGarnishmentMilestoneViews(new List<WeightedGarnishmentMilestone> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        WeightedGarnishmentMilestoneViewShowsTheStatusInCapitals();
    }
}
