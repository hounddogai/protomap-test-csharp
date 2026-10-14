namespace Acme.Workforce.Workplace.Tests.FloorPlan.Agreement;

using Acme.Workforce.Workplace.FloorPlan.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class LegacyFloorPlanRuleChecks
{
    public static void LegacyFloorPlanRuleStartsInCreated()
    {
        var item = LegacyFloorPlanRule.NewLegacyFloorPlanRule("code-234", "label-3");
        Check.Equal(item.Status, LegacyFloorPlanRuleStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyFloorPlanRuleMovesToRetired()
    {
        var item = LegacyFloorPlanRule.NewLegacyFloorPlanRule("code-234", "label-3");
        try
        {
            item.MoveTo(LegacyFloorPlanRuleStatus.Retired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyFloorPlanRuleStatus.Retired, "status");
    }

    public static void LegacyFloorPlanRuleRejectsMovesFromRetired()
    {
        var item = LegacyFloorPlanRule.NewLegacyFloorPlanRule("code-234", "label-3");
        item.Status = LegacyFloorPlanRuleStatus.Retired;
        Check.Fails(() => item.MoveTo(LegacyFloorPlanRuleStatus.Created), "move");
    }

    public static void RunAll()
    {
        LegacyFloorPlanRuleStartsInCreated();
        LegacyFloorPlanRuleMovesToRetired();
        LegacyFloorPlanRuleRejectsMovesFromRetired();
    }
}
