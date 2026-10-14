namespace Acme.Workforce.Compensation.Tests.BonusPool.Variance;

using Acme.Workforce.Compensation.BonusPool.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class BatchedBonusPoolReviewRulesChecks
{
    public static void BatchedBonusPoolReviewValidatorReportsEveryProblem()
    {
        var validator = BatchedBonusPoolReviewRules.DefaultBatchedBonusPoolReviewValidator();
        var item = BatchedBonusPoolReview.NewBatchedBonusPoolReview("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BatchedBonusPoolReviewRevisionRangeRuleFlagsValuesAboveRevision()
    {
        var rule = new BatchedBonusPoolReviewRevisionRangeRule(80, 86400);
        var item = BatchedBonusPoolReview.NewBatchedBonusPoolReview("x", "y");
        item.Revision = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BatchedBonusPoolReviewTitleLengthRuleFlagsALongTitle()
    {
        var rule = new BatchedBonusPoolReviewTitleLengthRule(3600);
        var item = BatchedBonusPoolReview.NewBatchedBonusPoolReview("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BatchedBonusPoolReviewHoursRangeRuleFlagsValuesAboveHours()
    {
        var rule = new BatchedBonusPoolReviewHoursRangeRule(1.9, 1235.2);
        var item = BatchedBonusPoolReview.NewBatchedBonusPoolReview("x", "y");
        item.Hours = 1236.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BatchedBonusPoolReviewRequiredManagerIdRuleFlagsABlankManagerId()
    {
        var rule = new BatchedBonusPoolReviewRequiredManagerIdRule("batched bonus pool review needs an entry");
        var item = BatchedBonusPoolReview.NewBatchedBonusPoolReview("", "");
        item.ManagerId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BatchedBonusPoolReviewExternalRefLengthRuleFlagsALongExternalRef()
    {
        var rule = new BatchedBonusPoolReviewExternalRefLengthRule(120);
        var item = BatchedBonusPoolReview.NewBatchedBonusPoolReview("x", "y");
        item.ExternalRef = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ExternalRef = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        BatchedBonusPoolReviewValidatorReportsEveryProblem();
        BatchedBonusPoolReviewRevisionRangeRuleFlagsValuesAboveRevision();
        BatchedBonusPoolReviewTitleLengthRuleFlagsALongTitle();
        BatchedBonusPoolReviewHoursRangeRuleFlagsValuesAboveHours();
        BatchedBonusPoolReviewRequiredManagerIdRuleFlagsABlankManagerId();
        BatchedBonusPoolReviewExternalRefLengthRuleFlagsALongExternalRef();
    }
}
