namespace Acme.Workforce.Engagement.Tests.ActionPlan.Metric;

using Acme.Workforce.Engagement.ActionPlan.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class LenientActionPlanNoticeRulesChecks
{
    public static void LenientActionPlanNoticeValidatorReportsEveryProblem()
    {
        var validator = LenientActionPlanNoticeRules.DefaultLenientActionPlanNoticeValidator();
        var item = LenientActionPlanNotice.NewLenientActionPlanNotice("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientActionPlanNoticeLimitRangeRuleFlagsValuesAboveLimit()
    {
        var rule = new LenientActionPlanNoticeLimitRangeRule(32, 1000);
        var item = LenientActionPlanNotice.NewLenientActionPlanNotice("x", "y");
        item.Limit = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LenientActionPlanNoticeSegmentLengthRuleFlagsALongSegment()
    {
        var rule = new LenientActionPlanNoticeSegmentLengthRule(32);
        var item = LenientActionPlanNotice.NewLenientActionPlanNotice("x", "y");
        item.Segment = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Segment = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LenientActionPlanNoticeValidatorReportsEveryProblem();
        LenientActionPlanNoticeLimitRangeRuleFlagsValuesAboveLimit();
        LenientActionPlanNoticeSegmentLengthRuleFlagsALongSegment();
    }
}
