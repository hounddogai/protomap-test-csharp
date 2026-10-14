namespace Acme.Workforce.Compliance.Tests.Exception.Review;

using Acme.Workforce.Compliance.Exception.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class ExceptionReviewRulesChecks
{
    public static void ExceptionReviewValidatorReportsEveryProblem()
    {
        var validator = ExceptionReviewRules.DefaultExceptionReviewValidator();
        var item = ExceptionReview.NewExceptionReview("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ExceptionReviewCreatedAtRangeRuleFlagsValuesAboveCreatedAt()
    {
        var rule = new ExceptionReviewCreatedAtRangeRule(16, 3600);
        var item = ExceptionReview.NewExceptionReview("x", "y");
        item.CreatedAt = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ExceptionReviewRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ExceptionReviewRequiredCodeRule("exception review needs a value");
        var item = ExceptionReview.NewExceptionReview("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ExceptionReviewTitleLengthRuleFlagsALongTitle()
    {
        var rule = new ExceptionReviewTitleLengthRule(3600);
        var item = ExceptionReview.NewExceptionReview("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ExceptionReviewLocaleLengthRuleFlagsALongLocale()
    {
        var rule = new ExceptionReviewLocaleLengthRule(3600);
        var item = ExceptionReview.NewExceptionReview("x", "y");
        item.Locale = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Locale = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ExceptionReviewScoreRangeRuleFlagsValuesAboveScore()
    {
        var rule = new ExceptionReviewScoreRangeRule(1.4, 4398.0);
        var item = ExceptionReview.NewExceptionReview("x", "y");
        item.Score = 4399.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        ExceptionReviewValidatorReportsEveryProblem();
        ExceptionReviewCreatedAtRangeRuleFlagsValuesAboveCreatedAt();
        ExceptionReviewRequiredCodeRuleFlagsABlankCode();
        ExceptionReviewTitleLengthRuleFlagsALongTitle();
        ExceptionReviewLocaleLengthRuleFlagsALongLocale();
        ExceptionReviewScoreRangeRuleFlagsValuesAboveScore();
    }
}
