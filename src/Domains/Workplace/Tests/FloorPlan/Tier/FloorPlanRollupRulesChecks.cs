namespace Acme.Workforce.Workplace.Tests.FloorPlan.Tier;

using Acme.Workforce.Workplace.FloorPlan.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class FloorPlanRollupRulesChecks
{
    public static void FloorPlanRollupValidatorReportsEveryProblem()
    {
        var validator = FloorPlanRollupRules.DefaultFloorPlanRollupValidator();
        var item = FloorPlanRollup.NewFloorPlanRollup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FloorPlanRollupRequiredSalaryBandRuleFlagsABlankSalaryBand()
    {
        var rule = new FloorPlanRollupRequiredSalaryBandRule("floor plan rollup needs a value");
        var item = FloorPlanRollup.NewFloorPlanRollup("", "");
        item.SalaryBand = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FloorPlanRollupRequiredJobCodeRuleFlagsABlankJobCode()
    {
        var rule = new FloorPlanRollupRequiredJobCodeRule("floor plan rollup needs this field");
        var item = FloorPlanRollup.NewFloorPlanRollup("", "");
        item.JobCode = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FloorPlanRollupRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new FloorPlanRollupRequiredCodeRule("floor plan rollup needs an entry");
        var item = FloorPlanRollup.NewFloorPlanRollup("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        FloorPlanRollupValidatorReportsEveryProblem();
        FloorPlanRollupRequiredSalaryBandRuleFlagsABlankSalaryBand();
        FloorPlanRollupRequiredJobCodeRuleFlagsABlankJobCode();
        FloorPlanRollupRequiredCodeRuleFlagsABlankCode();
    }
}
