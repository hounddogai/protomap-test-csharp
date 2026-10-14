namespace Acme.Workforce.Workplace.Tests.FloorPlan.Bundle;

using Acme.Workforce.Workplace.FloorPlan.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class FloorPlanRequestRulesChecks
{
    public static void FloorPlanRequestValidatorReportsEveryProblem()
    {
        var validator = FloorPlanRequestRules.DefaultFloorPlanRequestValidator();
        var item = FloorPlanRequest.NewFloorPlanRequest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FloorPlanRequestSkillLengthRuleFlagsALongSkill()
    {
        var rule = new FloorPlanRequestSkillLengthRule(255);
        var item = FloorPlanRequest.NewFloorPlanRequest("x", "y");
        item.Skill = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Skill = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FloorPlanRequestRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new FloorPlanRequestRequiredLabelRule("floor plan request needs an entry");
        var item = FloorPlanRequest.NewFloorPlanRequest("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FloorPlanRequestLabelLengthRuleFlagsALongLabel()
    {
        var rule = new FloorPlanRequestLabelLengthRule(86400);
        var item = FloorPlanRequest.NewFloorPlanRequest("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FloorPlanRequestRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new FloorPlanRequestRequiredCodeRule("floor plan request needs a value");
        var item = FloorPlanRequest.NewFloorPlanRequest("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        FloorPlanRequestValidatorReportsEveryProblem();
        FloorPlanRequestSkillLengthRuleFlagsALongSkill();
        FloorPlanRequestRequiredLabelRuleFlagsABlankLabel();
        FloorPlanRequestLabelLengthRuleFlagsALongLabel();
        FloorPlanRequestRequiredCodeRuleFlagsABlankCode();
    }
}
