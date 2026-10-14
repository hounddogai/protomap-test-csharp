namespace Acme.Workforce.Workplace.Tests.FloorPlan.Rule;

using Acme.Workforce.Workplace.FloorPlan.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class FloorPlanRuleChecks
{
    public static void FloorPlanRuleStartsInDraft()
    {
        var item = FloorPlanRule.NewFloorPlanRule("code-920", "label-498");
        Check.Equal(item.Status, FloorPlanRuleStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FloorPlanRuleMovesToPending()
    {
        var item = FloorPlanRule.NewFloorPlanRule("code-920", "label-498");
        try
        {
            item.MoveTo(FloorPlanRuleStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FloorPlanRuleStatus.Pending, "status");
    }

    public static void FloorPlanRuleRejectsMovesFromClosed()
    {
        var item = FloorPlanRule.NewFloorPlanRule("code-920", "label-498");
        item.Status = FloorPlanRuleStatus.Closed;
        Check.Fails(() => item.MoveTo(FloorPlanRuleStatus.Draft), "move");
    }

    public static void RunAll()
    {
        FloorPlanRuleStartsInDraft();
        FloorPlanRuleMovesToPending();
        FloorPlanRuleRejectsMovesFromClosed();
    }
}
