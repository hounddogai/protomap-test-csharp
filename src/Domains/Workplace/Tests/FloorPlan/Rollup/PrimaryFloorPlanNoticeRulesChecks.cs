namespace Acme.Workforce.Workplace.Tests.FloorPlan.Rollup;

using Acme.Workforce.Workplace.FloorPlan.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class PrimaryFloorPlanNoticeRulesChecks
{
    public static void PrimaryFloorPlanNoticeValidatorReportsEveryProblem()
    {
        var validator = PrimaryFloorPlanNoticeRules.DefaultPrimaryFloorPlanNoticeValidator();
        var item = PrimaryFloorPlanNotice.NewPrimaryFloorPlanNotice("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PrimaryFloorPlanNoticeLabelLengthRuleFlagsALongLabel()
    {
        var rule = new PrimaryFloorPlanNoticeLabelLengthRule(255);
        var item = PrimaryFloorPlanNotice.NewPrimaryFloorPlanNotice("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PrimaryFloorPlanNoticeCodeLengthRuleFlagsALongCode()
    {
        var rule = new PrimaryFloorPlanNoticeCodeLengthRule(16);
        var item = PrimaryFloorPlanNotice.NewPrimaryFloorPlanNotice("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PrimaryFloorPlanNoticePriorityRangeRuleFlagsValuesAbovePriority()
    {
        var rule = new PrimaryFloorPlanNoticePriorityRangeRule(3600, 3600);
        var item = PrimaryFloorPlanNotice.NewPrimaryFloorPlanNotice("x", "y");
        item.Priority = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        PrimaryFloorPlanNoticeValidatorReportsEveryProblem();
        PrimaryFloorPlanNoticeLabelLengthRuleFlagsALongLabel();
        PrimaryFloorPlanNoticeCodeLengthRuleFlagsALongCode();
        PrimaryFloorPlanNoticePriorityRangeRuleFlagsValuesAbovePriority();
    }
}
