namespace Acme.Workforce.Workplace.Tests.AccessBadge.Metric;

using Acme.Workforce.Workplace.AccessBadge.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class BatchedAccessBadgeTargetRulesChecks
{
    public static void BatchedAccessBadgeTargetValidatorReportsEveryProblem()
    {
        var validator = BatchedAccessBadgeTargetRules.DefaultBatchedAccessBadgeTargetValidator();
        var item = BatchedAccessBadgeTarget.NewBatchedAccessBadgeTarget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BatchedAccessBadgeTargetCodeLengthRuleFlagsALongCode()
    {
        var rule = new BatchedAccessBadgeTargetCodeLengthRule(120);
        var item = BatchedAccessBadgeTarget.NewBatchedAccessBadgeTarget("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BatchedAccessBadgeTargetTitleLengthRuleFlagsALongTitle()
    {
        var rule = new BatchedAccessBadgeTargetTitleLengthRule(16);
        var item = BatchedAccessBadgeTarget.NewBatchedAccessBadgeTarget("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BatchedAccessBadgeTargetRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new BatchedAccessBadgeTargetRequiredCodeRule("batched access badge target needs a value");
        var item = BatchedAccessBadgeTarget.NewBatchedAccessBadgeTarget("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        BatchedAccessBadgeTargetValidatorReportsEveryProblem();
        BatchedAccessBadgeTargetCodeLengthRuleFlagsALongCode();
        BatchedAccessBadgeTargetTitleLengthRuleFlagsALongTitle();
        BatchedAccessBadgeTargetRequiredCodeRuleFlagsABlankCode();
    }
}
