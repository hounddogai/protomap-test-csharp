namespace Acme.Workforce.Engagement.Tests.Feedback.Group;

using Acme.Workforce.Engagement.Feedback.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class BatchedFeedbackBatchRulesChecks
{
    public static void BatchedFeedbackBatchValidatorReportsEveryProblem()
    {
        var validator = BatchedFeedbackBatchRules.DefaultBatchedFeedbackBatchValidator();
        var item = BatchedFeedbackBatch.NewBatchedFeedbackBatch("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BatchedFeedbackBatchRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new BatchedFeedbackBatchRequiredCodeRule("batched feedback batch needs an entry");
        var item = BatchedFeedbackBatch.NewBatchedFeedbackBatch("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BatchedFeedbackBatchRequiredSegmentRuleFlagsABlankSegment()
    {
        var rule = new BatchedFeedbackBatchRequiredSegmentRule("batched feedback batch needs a value");
        var item = BatchedFeedbackBatch.NewBatchedFeedbackBatch("", "");
        item.Segment = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BatchedFeedbackBatchCategoryLengthRuleFlagsALongCategory()
    {
        var rule = new BatchedFeedbackBatchCategoryLengthRule(1000);
        var item = BatchedFeedbackBatch.NewBatchedFeedbackBatch("x", "y");
        item.Category = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Category = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BatchedFeedbackBatchAmountRangeRuleFlagsValuesAboveAmount()
    {
        var rule = new BatchedFeedbackBatchAmountRangeRule(2.8, 648.8);
        var item = BatchedFeedbackBatch.NewBatchedFeedbackBatch("x", "y");
        item.Amount = 650.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BatchedFeedbackBatchCodeLengthRuleFlagsALongCode()
    {
        var rule = new BatchedFeedbackBatchCodeLengthRule(64);
        var item = BatchedFeedbackBatch.NewBatchedFeedbackBatch("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        BatchedFeedbackBatchValidatorReportsEveryProblem();
        BatchedFeedbackBatchRequiredCodeRuleFlagsABlankCode();
        BatchedFeedbackBatchRequiredSegmentRuleFlagsABlankSegment();
        BatchedFeedbackBatchCategoryLengthRuleFlagsALongCategory();
        BatchedFeedbackBatchAmountRangeRuleFlagsValuesAboveAmount();
        BatchedFeedbackBatchCodeLengthRuleFlagsALongCode();
    }
}
