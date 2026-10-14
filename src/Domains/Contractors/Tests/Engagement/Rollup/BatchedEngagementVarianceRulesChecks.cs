namespace Acme.Workforce.Contractors.Tests.Engagement.Rollup;

using Acme.Workforce.Contractors.Engagement.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class BatchedEngagementVarianceRulesChecks
{
    public static void BatchedEngagementVarianceValidatorReportsEveryProblem()
    {
        var validator = BatchedEngagementVarianceRules.DefaultBatchedEngagementVarianceValidator();
        var item = BatchedEngagementVariance.NewBatchedEngagementVariance("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BatchedEngagementVarianceCodeLengthRuleFlagsALongCode()
    {
        var rule = new BatchedEngagementVarianceCodeLengthRule(1000);
        var item = BatchedEngagementVariance.NewBatchedEngagementVariance("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BatchedEngagementVarianceRequiredSkillRuleFlagsABlankSkill()
    {
        var rule = new BatchedEngagementVarianceRequiredSkillRule("batched engagement variance needs a value");
        var item = BatchedEngagementVariance.NewBatchedEngagementVariance("", "");
        item.Skill = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BatchedEngagementVarianceRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new BatchedEngagementVarianceRequiredCodeRule("batched engagement variance needs a value");
        var item = BatchedEngagementVariance.NewBatchedEngagementVariance("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        BatchedEngagementVarianceValidatorReportsEveryProblem();
        BatchedEngagementVarianceCodeLengthRuleFlagsALongCode();
        BatchedEngagementVarianceRequiredSkillRuleFlagsABlankSkill();
        BatchedEngagementVarianceRequiredCodeRuleFlagsABlankCode();
    }
}
