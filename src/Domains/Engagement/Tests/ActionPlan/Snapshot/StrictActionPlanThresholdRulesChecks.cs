namespace Acme.Workforce.Engagement.Tests.ActionPlan.Snapshot;

using Acme.Workforce.Engagement.ActionPlan.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class StrictActionPlanThresholdRulesChecks
{
    public static void StrictActionPlanThresholdValidatorReportsEveryProblem()
    {
        var validator = StrictActionPlanThresholdRules.DefaultStrictActionPlanThresholdValidator();
        var item = StrictActionPlanThreshold.NewStrictActionPlanThreshold("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StrictActionPlanThresholdSegmentLengthRuleFlagsALongSegment()
    {
        var rule = new StrictActionPlanThresholdSegmentLengthRule(16);
        var item = StrictActionPlanThreshold.NewStrictActionPlanThreshold("x", "y");
        item.Segment = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Segment = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StrictActionPlanThresholdRequiredSegmentRuleFlagsABlankSegment()
    {
        var rule = new StrictActionPlanThresholdRequiredSegmentRule("strict action plan threshold needs an entry");
        var item = StrictActionPlanThreshold.NewStrictActionPlanThreshold("", "");
        item.Segment = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StrictActionPlanThresholdNameLengthRuleFlagsALongName()
    {
        var rule = new StrictActionPlanThresholdNameLengthRule(3600);
        var item = StrictActionPlanThreshold.NewStrictActionPlanThreshold("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StrictActionPlanThresholdRequiredCategoryRuleFlagsABlankCategory()
    {
        var rule = new StrictActionPlanThresholdRequiredCategoryRule("strict action plan threshold needs this field");
        var item = StrictActionPlanThreshold.NewStrictActionPlanThreshold("", "");
        item.Category = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StrictActionPlanThresholdRateRangeRuleFlagsValuesAboveRate()
    {
        var rule = new StrictActionPlanThresholdRateRangeRule(2.2, 3750.4);
        var item = StrictActionPlanThreshold.NewStrictActionPlanThreshold("x", "y");
        item.Rate = 3751.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void StrictActionPlanThresholdCodeLengthRuleFlagsALongCode()
    {
        var rule = new StrictActionPlanThresholdCodeLengthRule(80);
        var item = StrictActionPlanThreshold.NewStrictActionPlanThreshold("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        StrictActionPlanThresholdValidatorReportsEveryProblem();
        StrictActionPlanThresholdSegmentLengthRuleFlagsALongSegment();
        StrictActionPlanThresholdRequiredSegmentRuleFlagsABlankSegment();
        StrictActionPlanThresholdNameLengthRuleFlagsALongName();
        StrictActionPlanThresholdRequiredCategoryRuleFlagsABlankCategory();
        StrictActionPlanThresholdRateRangeRuleFlagsValuesAboveRate();
        StrictActionPlanThresholdCodeLengthRuleFlagsALongCode();
    }
}
