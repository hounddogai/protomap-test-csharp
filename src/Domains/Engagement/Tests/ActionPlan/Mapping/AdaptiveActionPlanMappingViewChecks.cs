namespace Acme.Workforce.Engagement.Tests.ActionPlan.Mapping;

using Acme.Workforce.Engagement.ActionPlan.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class AdaptiveActionPlanMappingViewChecks
{
    public static void AdaptiveActionPlanMappingViewShowsTheStatusInCapitals()
    {
        var item = AdaptiveActionPlanMapping.NewAdaptiveActionPlanMapping("code-575", "title-100");
        var view = AdaptiveActionPlanMappingView.ToAdaptiveActionPlanMappingView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AdaptiveActionPlanMappingView.ToAdaptiveActionPlanMappingViews(new List<AdaptiveActionPlanMapping> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AdaptiveActionPlanMappingViewShowsTheStatusInCapitals();
    }
}
