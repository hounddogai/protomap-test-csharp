namespace Acme.Workforce.Workplace.Tests.FloorPlan.Rule;

using Acme.Workforce.Workplace.FloorPlan.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class FloorPlanRuleViewChecks
{
    public static void FloorPlanRuleViewShowsTheStatusInCapitals()
    {
        var item = FloorPlanRule.NewFloorPlanRule("code-920", "label-498");
        var view = FloorPlanRuleView.ToFloorPlanRuleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(FloorPlanRuleView.ToFloorPlanRuleViews(new List<FloorPlanRule> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        FloorPlanRuleViewShowsTheStatusInCapitals();
    }
}
