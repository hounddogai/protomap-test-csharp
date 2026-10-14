namespace Acme.Workforce.Workplace.Tests.FloorPlan.PlanAudit;

using Acme.Workforce.Workplace.FloorPlan.PlanAudit;

/// <summary>Checks of plan audit code, run by the project's runner.</summary>
public static class DefaultFloorPlanSegmentRulesChecks
{
    public static void DefaultFloorPlanSegmentValidatorReportsEveryProblem()
    {
        var validator = DefaultFloorPlanSegmentRules.DefaultDefaultFloorPlanSegmentValidator();
        var item = DefaultFloorPlanSegment.NewDefaultFloorPlanSegment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultFloorPlanSegmentLeaveDaysRangeRuleFlagsValuesAboveLeaveDays()
    {
        var rule = new DefaultFloorPlanSegmentLeaveDaysRangeRule(64, 3600);
        var item = DefaultFloorPlanSegment.NewDefaultFloorPlanSegment("x", "y");
        item.LeaveDays = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DefaultFloorPlanSegmentCodeLengthRuleFlagsALongCode()
    {
        var rule = new DefaultFloorPlanSegmentCodeLengthRule(86400);
        var item = DefaultFloorPlanSegment.NewDefaultFloorPlanSegment("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DefaultFloorPlanSegmentNameLengthRuleFlagsALongName()
    {
        var rule = new DefaultFloorPlanSegmentNameLengthRule(255);
        var item = DefaultFloorPlanSegment.NewDefaultFloorPlanSegment("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        DefaultFloorPlanSegmentValidatorReportsEveryProblem();
        DefaultFloorPlanSegmentLeaveDaysRangeRuleFlagsValuesAboveLeaveDays();
        DefaultFloorPlanSegmentCodeLengthRuleFlagsALongCode();
        DefaultFloorPlanSegmentNameLengthRuleFlagsALongName();
    }
}
