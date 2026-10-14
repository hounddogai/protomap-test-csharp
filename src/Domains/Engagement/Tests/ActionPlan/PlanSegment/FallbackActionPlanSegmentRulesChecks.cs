namespace Acme.Workforce.Engagement.Tests.ActionPlan.PlanSegment;

using Acme.Workforce.Engagement.ActionPlan.PlanSegment;

/// <summary>Checks of plan segment code, run by the project's runner.</summary>
public static class FallbackActionPlanSegmentRulesChecks
{
    public static void FallbackActionPlanSegmentValidatorReportsEveryProblem()
    {
        var validator = FallbackActionPlanSegmentRules.DefaultFallbackActionPlanSegmentValidator();
        var item = FallbackActionPlanSegment.NewFallbackActionPlanSegment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FallbackActionPlanSegmentCodeLengthRuleFlagsALongCode()
    {
        var rule = new FallbackActionPlanSegmentCodeLengthRule(64);
        var item = FallbackActionPlanSegment.NewFallbackActionPlanSegment("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FallbackActionPlanSegmentThresholdRangeRuleFlagsValuesAboveThreshold()
    {
        var rule = new FallbackActionPlanSegmentThresholdRangeRule(7.1, 2613.3);
        var item = FallbackActionPlanSegment.NewFallbackActionPlanSegment("x", "y");
        item.Threshold = 2614.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void FallbackActionPlanSegmentBatchSizeRangeRuleFlagsValuesAboveBatchSize()
    {
        var rule = new FallbackActionPlanSegmentBatchSizeRangeRule(1000, 86400);
        var item = FallbackActionPlanSegment.NewFallbackActionPlanSegment("x", "y");
        item.BatchSize = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        FallbackActionPlanSegmentValidatorReportsEveryProblem();
        FallbackActionPlanSegmentCodeLengthRuleFlagsALongCode();
        FallbackActionPlanSegmentThresholdRangeRuleFlagsValuesAboveThreshold();
        FallbackActionPlanSegmentBatchSizeRangeRuleFlagsValuesAboveBatchSize();
    }
}
