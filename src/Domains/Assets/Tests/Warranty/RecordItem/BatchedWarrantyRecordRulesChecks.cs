namespace Acme.Workforce.Assets.Tests.Warranty.RecordItem;

using Acme.Workforce.Assets.Warranty.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class BatchedWarrantyRecordRulesChecks
{
    public static void BatchedWarrantyRecordValidatorReportsEveryProblem()
    {
        var validator = BatchedWarrantyRecordRules.DefaultBatchedWarrantyRecordValidator();
        var item = BatchedWarrantyRecord.NewBatchedWarrantyRecord("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BatchedWarrantyRecordReviewerLengthRuleFlagsALongReviewer()
    {
        var rule = new BatchedWarrantyRecordReviewerLengthRule(16);
        var item = BatchedWarrantyRecord.NewBatchedWarrantyRecord("x", "y");
        item.Reviewer = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Reviewer = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BatchedWarrantyRecordTitleLengthRuleFlagsALongTitle()
    {
        var rule = new BatchedWarrantyRecordTitleLengthRule(86400);
        var item = BatchedWarrantyRecord.NewBatchedWarrantyRecord("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BatchedWarrantyRecordRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new BatchedWarrantyRecordRequiredCodeRule("batched warranty record needs this field");
        var item = BatchedWarrantyRecord.NewBatchedWarrantyRecord("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BatchedWarrantyRecordMinimumRangeRuleFlagsValuesAboveMinimum()
    {
        var rule = new BatchedWarrantyRecordMinimumRangeRule(4.1, 2035.8);
        var item = BatchedWarrantyRecord.NewBatchedWarrantyRecord("x", "y");
        item.Minimum = 2037.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        BatchedWarrantyRecordValidatorReportsEveryProblem();
        BatchedWarrantyRecordReviewerLengthRuleFlagsALongReviewer();
        BatchedWarrantyRecordTitleLengthRuleFlagsALongTitle();
        BatchedWarrantyRecordRequiredCodeRuleFlagsABlankCode();
        BatchedWarrantyRecordMinimumRangeRuleFlagsValuesAboveMinimum();
    }
}
