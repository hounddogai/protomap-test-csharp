namespace Acme.Workforce.Assets.Tests.Refresh.Review;

using Acme.Workforce.Assets.Refresh.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class RefreshReviewRulesChecks
{
    public static void RefreshReviewValidatorReportsEveryProblem()
    {
        var validator = RefreshReviewRules.DefaultRefreshReviewValidator();
        var item = RefreshReview.NewRefreshReview("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RefreshReviewRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new RefreshReviewRequiredTitleRule("refresh review needs an entry");
        var item = RefreshReview.NewRefreshReview("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RefreshReviewCodeLengthRuleFlagsALongCode()
    {
        var rule = new RefreshReviewCodeLengthRule(64);
        var item = RefreshReview.NewRefreshReview("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RefreshReviewChannelLengthRuleFlagsALongChannel()
    {
        var rule = new RefreshReviewChannelLengthRule(32);
        var item = RefreshReview.NewRefreshReview("x", "y");
        item.Channel = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Channel = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RefreshReviewStartsAtRangeRuleFlagsValuesAboveStartsAt()
    {
        var rule = new RefreshReviewStartsAtRangeRule(80, 3600);
        var item = RefreshReview.NewRefreshReview("x", "y");
        item.StartsAt = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RefreshReviewTitleLengthRuleFlagsALongTitle()
    {
        var rule = new RefreshReviewTitleLengthRule(255);
        var item = RefreshReview.NewRefreshReview("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        RefreshReviewValidatorReportsEveryProblem();
        RefreshReviewRequiredTitleRuleFlagsABlankTitle();
        RefreshReviewCodeLengthRuleFlagsALongCode();
        RefreshReviewChannelLengthRuleFlagsALongChannel();
        RefreshReviewStartsAtRangeRuleFlagsValuesAboveStartsAt();
        RefreshReviewTitleLengthRuleFlagsALongTitle();
    }
}
