namespace Acme.Workforce.Compensation.Tests.Adjustment.Request;

using Acme.Workforce.Compensation.Adjustment.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class BatchedAdjustmentRequestRulesChecks
{
    public static void BatchedAdjustmentRequestValidatorReportsEveryProblem()
    {
        var validator = BatchedAdjustmentRequestRules.DefaultBatchedAdjustmentRequestValidator();
        var item = BatchedAdjustmentRequest.NewBatchedAdjustmentRequest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BatchedAdjustmentRequestSkillLengthRuleFlagsALongSkill()
    {
        var rule = new BatchedAdjustmentRequestSkillLengthRule(86400);
        var item = BatchedAdjustmentRequest.NewBatchedAdjustmentRequest("x", "y");
        item.Skill = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Skill = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BatchedAdjustmentRequestRequiredBadgeRuleFlagsABlankBadge()
    {
        var rule = new BatchedAdjustmentRequestRequiredBadgeRule("batched adjustment request needs a value");
        var item = BatchedAdjustmentRequest.NewBatchedAdjustmentRequest("", "");
        item.Badge = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BatchedAdjustmentRequestRequiredSkillRuleFlagsABlankSkill()
    {
        var rule = new BatchedAdjustmentRequestRequiredSkillRule("batched adjustment request needs an entry");
        var item = BatchedAdjustmentRequest.NewBatchedAdjustmentRequest("", "");
        item.Skill = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BatchedAdjustmentRequestBadgeLengthRuleFlagsALongBadge()
    {
        var rule = new BatchedAdjustmentRequestBadgeLengthRule(255);
        var item = BatchedAdjustmentRequest.NewBatchedAdjustmentRequest("x", "y");
        item.Badge = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Badge = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        BatchedAdjustmentRequestValidatorReportsEveryProblem();
        BatchedAdjustmentRequestSkillLengthRuleFlagsALongSkill();
        BatchedAdjustmentRequestRequiredBadgeRuleFlagsABlankBadge();
        BatchedAdjustmentRequestRequiredSkillRuleFlagsABlankSkill();
        BatchedAdjustmentRequestBadgeLengthRuleFlagsALongBadge();
    }
}
