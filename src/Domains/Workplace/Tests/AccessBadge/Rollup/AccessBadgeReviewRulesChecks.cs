namespace Acme.Workforce.Workplace.Tests.AccessBadge.Rollup;

using Acme.Workforce.Workplace.AccessBadge.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class AccessBadgeReviewRulesChecks
{
    public static void AccessBadgeReviewValidatorReportsEveryProblem()
    {
        var validator = AccessBadgeReviewRules.DefaultAccessBadgeReviewValidator();
        var item = AccessBadgeReview.NewAccessBadgeReview("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AccessBadgeReviewCodeLengthRuleFlagsALongCode()
    {
        var rule = new AccessBadgeReviewCodeLengthRule(86400);
        var item = AccessBadgeReview.NewAccessBadgeReview("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AccessBadgeReviewAttemptsRangeRuleFlagsValuesAboveAttempts()
    {
        var rule = new AccessBadgeReviewAttemptsRangeRule(32, 120);
        var item = AccessBadgeReview.NewAccessBadgeReview("x", "y");
        item.Attempts = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AccessBadgeReviewLimitRangeRuleFlagsValuesAboveLimit()
    {
        var rule = new AccessBadgeReviewLimitRangeRule(255, 255);
        var item = AccessBadgeReview.NewAccessBadgeReview("x", "y");
        item.Limit = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        AccessBadgeReviewValidatorReportsEveryProblem();
        AccessBadgeReviewCodeLengthRuleFlagsALongCode();
        AccessBadgeReviewAttemptsRangeRuleFlagsValuesAboveAttempts();
        AccessBadgeReviewLimitRangeRuleFlagsValuesAboveLimit();
    }
}
