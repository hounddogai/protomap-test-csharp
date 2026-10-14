namespace Acme.Workforce.Engagement.Tests.ActionPlan.Draft;

using Acme.Workforce.Engagement.ActionPlan.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class PrimaryActionPlanMappingViewChecks
{
    public static void PrimaryActionPlanMappingViewShowsTheStatusInCapitals()
    {
        var item = PrimaryActionPlanMapping.NewPrimaryActionPlanMapping("code-234", "title-997");
        var view = PrimaryActionPlanMappingView.ToPrimaryActionPlanMappingView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PrimaryActionPlanMappingView.ToPrimaryActionPlanMappingViews(new List<PrimaryActionPlanMapping> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PrimaryActionPlanMappingViewShowsTheStatusInCapitals();
    }
}
