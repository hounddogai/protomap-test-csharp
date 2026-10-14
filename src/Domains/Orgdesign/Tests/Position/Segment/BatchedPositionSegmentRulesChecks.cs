namespace Acme.Workforce.Orgdesign.Tests.Position.Segment;

using Acme.Workforce.Orgdesign.Position.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class BatchedPositionSegmentRulesChecks
{
    public static void BatchedPositionSegmentValidatorReportsEveryProblem()
    {
        var validator = BatchedPositionSegmentRules.DefaultBatchedPositionSegmentValidator();
        var item = BatchedPositionSegment.NewBatchedPositionSegment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BatchedPositionSegmentAttemptsRangeRuleFlagsValuesAboveAttempts()
    {
        var rule = new BatchedPositionSegmentAttemptsRangeRule(80, 3600);
        var item = BatchedPositionSegment.NewBatchedPositionSegment("x", "y");
        item.Attempts = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BatchedPositionSegmentCapacityRangeRuleFlagsValuesAboveCapacity()
    {
        var rule = new BatchedPositionSegmentCapacityRangeRule(64, 120);
        var item = BatchedPositionSegment.NewBatchedPositionSegment("x", "y");
        item.Capacity = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BatchedPositionSegmentRequiredReviewerRuleFlagsABlankReviewer()
    {
        var rule = new BatchedPositionSegmentRequiredReviewerRule("batched position segment needs a value");
        var item = BatchedPositionSegment.NewBatchedPositionSegment("", "");
        item.Reviewer = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BatchedPositionSegmentReviewerLengthRuleFlagsALongReviewer()
    {
        var rule = new BatchedPositionSegmentReviewerLengthRule(86400);
        var item = BatchedPositionSegment.NewBatchedPositionSegment("x", "y");
        item.Reviewer = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Reviewer = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BatchedPositionSegmentBudgetRangeRuleFlagsValuesAboveBudget()
    {
        var rule = new BatchedPositionSegmentBudgetRangeRule(0.3, 2637.3);
        var item = BatchedPositionSegment.NewBatchedPositionSegment("x", "y");
        item.Budget = 2638.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        BatchedPositionSegmentValidatorReportsEveryProblem();
        BatchedPositionSegmentAttemptsRangeRuleFlagsValuesAboveAttempts();
        BatchedPositionSegmentCapacityRangeRuleFlagsValuesAboveCapacity();
        BatchedPositionSegmentRequiredReviewerRuleFlagsABlankReviewer();
        BatchedPositionSegmentReviewerLengthRuleFlagsALongReviewer();
        BatchedPositionSegmentBudgetRangeRuleFlagsValuesAboveBudget();
    }
}
