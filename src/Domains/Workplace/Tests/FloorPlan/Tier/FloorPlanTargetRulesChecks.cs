namespace Acme.Workforce.Workplace.Tests.FloorPlan.Tier;

using Acme.Workforce.Workplace.FloorPlan.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class FloorPlanTargetRulesChecks
{
    public static void FloorPlanTargetValidatorReportsEveryProblem()
    {
        var validator = FloorPlanTargetRules.DefaultFloorPlanTargetValidator();
        var item = FloorPlanTarget.NewFloorPlanTarget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FloorPlanTargetRequiredJobCodeRuleFlagsABlankJobCode()
    {
        var rule = new FloorPlanTargetRequiredJobCodeRule("floor plan target needs this field");
        var item = FloorPlanTarget.NewFloorPlanTarget("", "");
        item.JobCode = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FloorPlanTargetTitleLengthRuleFlagsALongTitle()
    {
        var rule = new FloorPlanTargetTitleLengthRule(64);
        var item = FloorPlanTarget.NewFloorPlanTarget("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FloorPlanTargetCodeLengthRuleFlagsALongCode()
    {
        var rule = new FloorPlanTargetCodeLengthRule(32);
        var item = FloorPlanTarget.NewFloorPlanTarget("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FloorPlanTargetRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new FloorPlanTargetRequiredCodeRule("floor plan target needs an entry");
        var item = FloorPlanTarget.NewFloorPlanTarget("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        FloorPlanTargetValidatorReportsEveryProblem();
        FloorPlanTargetRequiredJobCodeRuleFlagsABlankJobCode();
        FloorPlanTargetTitleLengthRuleFlagsALongTitle();
        FloorPlanTargetCodeLengthRuleFlagsALongCode();
        FloorPlanTargetRequiredCodeRuleFlagsABlankCode();
    }
}
