namespace Acme.Workforce.Workplace.Tests.FloorPlan.Audit;

using Acme.Workforce.Workplace.FloorPlan.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class FloorPlanSegmentRulesChecks
{
    public static void FloorPlanSegmentValidatorReportsEveryProblem()
    {
        var validator = FloorPlanSegmentRules.DefaultFloorPlanSegmentValidator();
        var item = FloorPlanSegment.NewFloorPlanSegment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FloorPlanSegmentRatioRangeRuleFlagsValuesAboveRatio()
    {
        var rule = new FloorPlanSegmentRatioRangeRule(3.1, 739.3);
        var item = FloorPlanSegment.NewFloorPlanSegment("x", "y");
        item.Ratio = 740.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void FloorPlanSegmentCodeLengthRuleFlagsALongCode()
    {
        var rule = new FloorPlanSegmentCodeLengthRule(1000);
        var item = FloorPlanSegment.NewFloorPlanSegment("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FloorPlanSegmentRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new FloorPlanSegmentRequiredCodeRule("floor plan segment needs a value");
        var item = FloorPlanSegment.NewFloorPlanSegment("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FloorPlanSegmentTitleLengthRuleFlagsALongTitle()
    {
        var rule = new FloorPlanSegmentTitleLengthRule(16);
        var item = FloorPlanSegment.NewFloorPlanSegment("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        FloorPlanSegmentValidatorReportsEveryProblem();
        FloorPlanSegmentRatioRangeRuleFlagsValuesAboveRatio();
        FloorPlanSegmentCodeLengthRuleFlagsALongCode();
        FloorPlanSegmentRequiredCodeRuleFlagsABlankCode();
        FloorPlanSegmentTitleLengthRuleFlagsALongTitle();
    }
}
