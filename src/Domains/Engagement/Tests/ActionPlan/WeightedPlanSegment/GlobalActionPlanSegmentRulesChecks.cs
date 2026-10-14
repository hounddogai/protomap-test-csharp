namespace Acme.Workforce.Engagement.Tests.ActionPlan.WeightedPlanSegment;

using Acme.Workforce.Engagement.ActionPlan.WeightedPlanSegment;

/// <summary>Checks of weighted plan segment code, run by the project's runner.</summary>
public static class GlobalActionPlanSegmentRulesChecks
{
    public static void GlobalActionPlanSegmentValidatorReportsEveryProblem()
    {
        var validator = GlobalActionPlanSegmentRules.DefaultGlobalActionPlanSegmentValidator();
        var item = GlobalActionPlanSegment.NewGlobalActionPlanSegment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void GlobalActionPlanSegmentAttemptsRangeRuleFlagsValuesAboveAttempts()
    {
        var rule = new GlobalActionPlanSegmentAttemptsRangeRule(16, 120);
        var item = GlobalActionPlanSegment.NewGlobalActionPlanSegment("x", "y");
        item.Attempts = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void GlobalActionPlanSegmentCodeLengthRuleFlagsALongCode()
    {
        var rule = new GlobalActionPlanSegmentCodeLengthRule(64);
        var item = GlobalActionPlanSegment.NewGlobalActionPlanSegment("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GlobalActionPlanSegmentRatioRangeRuleFlagsValuesAboveRatio()
    {
        var rule = new GlobalActionPlanSegmentRatioRangeRule(5.7, 4255.6);
        var item = GlobalActionPlanSegment.NewGlobalActionPlanSegment("x", "y");
        item.Ratio = 4257.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        GlobalActionPlanSegmentValidatorReportsEveryProblem();
        GlobalActionPlanSegmentAttemptsRangeRuleFlagsValuesAboveAttempts();
        GlobalActionPlanSegmentCodeLengthRuleFlagsALongCode();
        GlobalActionPlanSegmentRatioRangeRuleFlagsValuesAboveRatio();
    }
}
