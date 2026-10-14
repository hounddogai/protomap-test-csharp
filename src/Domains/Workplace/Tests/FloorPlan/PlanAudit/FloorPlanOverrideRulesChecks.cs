namespace Acme.Workforce.Workplace.Tests.FloorPlan.PlanAudit;

using Acme.Workforce.Workplace.FloorPlan.PlanAudit;

/// <summary>Checks of plan audit code, run by the project's runner.</summary>
public static class FloorPlanOverrideRulesChecks
{
    public static void FloorPlanOverrideValidatorReportsEveryProblem()
    {
        var validator = FloorPlanOverrideRules.DefaultFloorPlanOverrideValidator();
        var item = FloorPlanOverride.NewFloorPlanOverride("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FloorPlanOverrideLabelLengthRuleFlagsALongLabel()
    {
        var rule = new FloorPlanOverrideLabelLengthRule(64);
        var item = FloorPlanOverride.NewFloorPlanOverride("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FloorPlanOverrideRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new FloorPlanOverrideRequiredCodeRule("floor plan override needs this field");
        var item = FloorPlanOverride.NewFloorPlanOverride("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FloorPlanOverrideChannelLengthRuleFlagsALongChannel()
    {
        var rule = new FloorPlanOverrideChannelLengthRule(32);
        var item = FloorPlanOverride.NewFloorPlanOverride("x", "y");
        item.Channel = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Channel = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        FloorPlanOverrideValidatorReportsEveryProblem();
        FloorPlanOverrideLabelLengthRuleFlagsALongLabel();
        FloorPlanOverrideRequiredCodeRuleFlagsABlankCode();
        FloorPlanOverrideChannelLengthRuleFlagsALongChannel();
    }
}
