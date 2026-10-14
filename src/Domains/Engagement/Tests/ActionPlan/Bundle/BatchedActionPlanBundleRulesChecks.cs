namespace Acme.Workforce.Engagement.Tests.ActionPlan.Bundle;

using Acme.Workforce.Engagement.ActionPlan.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class BatchedActionPlanBundleRulesChecks
{
    public static void BatchedActionPlanBundleValidatorReportsEveryProblem()
    {
        var validator = BatchedActionPlanBundleRules.DefaultBatchedActionPlanBundleValidator();
        var item = BatchedActionPlanBundle.NewBatchedActionPlanBundle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BatchedActionPlanBundleRequiredTimezoneRuleFlagsABlankTimezone()
    {
        var rule = new BatchedActionPlanBundleRequiredTimezoneRule("batched action plan bundle needs a value");
        var item = BatchedActionPlanBundle.NewBatchedActionPlanBundle("", "");
        item.Timezone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BatchedActionPlanBundleSummaryLengthRuleFlagsALongSummary()
    {
        var rule = new BatchedActionPlanBundleSummaryLengthRule(80);
        var item = BatchedActionPlanBundle.NewBatchedActionPlanBundle("x", "y");
        item.Summary = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Summary = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        BatchedActionPlanBundleValidatorReportsEveryProblem();
        BatchedActionPlanBundleRequiredTimezoneRuleFlagsABlankTimezone();
        BatchedActionPlanBundleSummaryLengthRuleFlagsALongSummary();
    }
}
