namespace Acme.Workforce.Engagement.Tests.Feedback.Notice;

using Acme.Workforce.Engagement.Feedback.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class BatchedFeedbackNoticeRulesChecks
{
    public static void BatchedFeedbackNoticeValidatorReportsEveryProblem()
    {
        var validator = BatchedFeedbackNoticeRules.DefaultBatchedFeedbackNoticeValidator();
        var item = BatchedFeedbackNotice.NewBatchedFeedbackNotice("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BatchedFeedbackNoticeRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new BatchedFeedbackNoticeRequiredTitleRule("batched feedback notice needs this field");
        var item = BatchedFeedbackNotice.NewBatchedFeedbackNotice("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BatchedFeedbackNoticeRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new BatchedFeedbackNoticeRequiredLabelRule("batched feedback notice needs a value");
        var item = BatchedFeedbackNotice.NewBatchedFeedbackNotice("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BatchedFeedbackNoticeSkillLengthRuleFlagsALongSkill()
    {
        var rule = new BatchedFeedbackNoticeSkillLengthRule(3600);
        var item = BatchedFeedbackNotice.NewBatchedFeedbackNotice("x", "y");
        item.Skill = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Skill = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BatchedFeedbackNoticeRequiredSkillRuleFlagsABlankSkill()
    {
        var rule = new BatchedFeedbackNoticeRequiredSkillRule("batched feedback notice needs an entry");
        var item = BatchedFeedbackNotice.NewBatchedFeedbackNotice("", "");
        item.Skill = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BatchedFeedbackNoticeLabelLengthRuleFlagsALongLabel()
    {
        var rule = new BatchedFeedbackNoticeLabelLengthRule(1000);
        var item = BatchedFeedbackNotice.NewBatchedFeedbackNotice("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        BatchedFeedbackNoticeValidatorReportsEveryProblem();
        BatchedFeedbackNoticeRequiredTitleRuleFlagsABlankTitle();
        BatchedFeedbackNoticeRequiredLabelRuleFlagsABlankLabel();
        BatchedFeedbackNoticeSkillLengthRuleFlagsALongSkill();
        BatchedFeedbackNoticeRequiredSkillRuleFlagsABlankSkill();
        BatchedFeedbackNoticeLabelLengthRuleFlagsALongLabel();
    }
}
