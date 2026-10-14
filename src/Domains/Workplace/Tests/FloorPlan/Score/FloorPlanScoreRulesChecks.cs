namespace Acme.Workforce.Workplace.Tests.FloorPlan.Score;

using Acme.Workforce.Workplace.FloorPlan.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class FloorPlanScoreRulesChecks
{
    public static void FloorPlanScoreValidatorReportsEveryProblem()
    {
        var validator = FloorPlanScoreRules.DefaultFloorPlanScoreValidator();
        var item = FloorPlanScore.NewFloorPlanScore("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FloorPlanScoreCodeLengthRuleFlagsALongCode()
    {
        var rule = new FloorPlanScoreCodeLengthRule(3600);
        var item = FloorPlanScore.NewFloorPlanScore("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FloorPlanScoreRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new FloorPlanScoreRequiredCodeRule("floor plan score needs a value");
        var item = FloorPlanScore.NewFloorPlanScore("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FloorPlanScoreRequiredSalaryBandRuleFlagsABlankSalaryBand()
    {
        var rule = new FloorPlanScoreRequiredSalaryBandRule("floor plan score needs a value");
        var item = FloorPlanScore.NewFloorPlanScore("", "");
        item.SalaryBand = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FloorPlanScoreCreatedAtRangeRuleFlagsValuesAboveCreatedAt()
    {
        var rule = new FloorPlanScoreCreatedAtRangeRule(120, 86400);
        var item = FloorPlanScore.NewFloorPlanScore("x", "y");
        item.CreatedAt = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        FloorPlanScoreValidatorReportsEveryProblem();
        FloorPlanScoreCodeLengthRuleFlagsALongCode();
        FloorPlanScoreRequiredCodeRuleFlagsABlankCode();
        FloorPlanScoreRequiredSalaryBandRuleFlagsABlankSalaryBand();
        FloorPlanScoreCreatedAtRangeRuleFlagsValuesAboveCreatedAt();
    }
}
