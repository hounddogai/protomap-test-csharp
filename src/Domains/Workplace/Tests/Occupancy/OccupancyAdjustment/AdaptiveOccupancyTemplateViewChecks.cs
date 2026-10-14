namespace Acme.Workforce.Workplace.Tests.Occupancy.OccupancyAdjustment;

using Acme.Workforce.Workplace.Occupancy.OccupancyAdjustment;

/// <summary>Checks of occupancy adjustment code, run by the project's runner.</summary>
public static class AdaptiveOccupancyTemplateViewChecks
{
    public static void AdaptiveOccupancyTemplateViewShowsTheStatusInCapitals()
    {
        var item = AdaptiveOccupancyTemplate.NewAdaptiveOccupancyTemplate("code-680", "label-203");
        var view = AdaptiveOccupancyTemplateView.ToAdaptiveOccupancyTemplateView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AdaptiveOccupancyTemplateView.ToAdaptiveOccupancyTemplateViews(new List<AdaptiveOccupancyTemplate> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AdaptiveOccupancyTemplateViewShowsTheStatusInCapitals();
    }
}
