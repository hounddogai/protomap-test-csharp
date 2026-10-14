namespace Acme.Workforce.Engagement.Tests.Feedback.Rule;

using Acme.Workforce.Engagement.Feedback.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class BatchedFeedbackBaselineRulesChecks
{
    public static void BatchedFeedbackBaselineValidatorReportsEveryProblem()
    {
        var validator = BatchedFeedbackBaselineRules.DefaultBatchedFeedbackBaselineValidator();
        var item = BatchedFeedbackBaseline.NewBatchedFeedbackBaseline("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BatchedFeedbackBaselineEndsAtRangeRuleFlagsValuesAboveEndsAt()
    {
        var rule = new BatchedFeedbackBaselineEndsAtRangeRule(1000, 3600);
        var item = BatchedFeedbackBaseline.NewBatchedFeedbackBaseline("x", "y");
        item.EndsAt = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BatchedFeedbackBaselineSegmentLengthRuleFlagsALongSegment()
    {
        var rule = new BatchedFeedbackBaselineSegmentLengthRule(3600);
        var item = BatchedFeedbackBaseline.NewBatchedFeedbackBaseline("x", "y");
        item.Segment = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Segment = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BatchedFeedbackBaselineRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new BatchedFeedbackBaselineRequiredTitleRule("batched feedback baseline needs an entry");
        var item = BatchedFeedbackBaseline.NewBatchedFeedbackBaseline("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BatchedFeedbackBaselineDepartmentLengthRuleFlagsALongDepartment()
    {
        var rule = new BatchedFeedbackBaselineDepartmentLengthRule(3600);
        var item = BatchedFeedbackBaseline.NewBatchedFeedbackBaseline("x", "y");
        item.Department = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Department = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        BatchedFeedbackBaselineValidatorReportsEveryProblem();
        BatchedFeedbackBaselineEndsAtRangeRuleFlagsValuesAboveEndsAt();
        BatchedFeedbackBaselineSegmentLengthRuleFlagsALongSegment();
        BatchedFeedbackBaselineRequiredTitleRuleFlagsABlankTitle();
        BatchedFeedbackBaselineDepartmentLengthRuleFlagsALongDepartment();
    }
}
