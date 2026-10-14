namespace Acme.Workforce.Workplace.Tests.FloorPlan.Agreement;

using Acme.Workforce.Workplace.FloorPlan.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class LegacyFloorPlanRuleViewChecks
{
    public static void LegacyFloorPlanRuleViewShowsTheStatusInCapitals()
    {
        var item = LegacyFloorPlanRule.NewLegacyFloorPlanRule("code-234", "label-3");
        var view = LegacyFloorPlanRuleView.ToLegacyFloorPlanRuleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LegacyFloorPlanRuleView.ToLegacyFloorPlanRuleViews(new List<LegacyFloorPlanRule> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LegacyFloorPlanRuleViewShowsTheStatusInCapitals();
    }
}
