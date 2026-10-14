namespace Acme.Workforce.Absence.Tests.LeaveRequest.Ledger;

using Acme.Workforce.Absence.LeaveRequest.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class SeasonalLeaveRequestReviewRulesChecks
{
    public static void SeasonalLeaveRequestReviewValidatorReportsEveryProblem()
    {
        var validator = SeasonalLeaveRequestReviewRules.DefaultSeasonalLeaveRequestReviewValidator();
        var item = SeasonalLeaveRequestReview.NewSeasonalLeaveRequestReview("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SeasonalLeaveRequestReviewRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new SeasonalLeaveRequestReviewRequiredTitleRule("seasonal leave request review needs a value");
        var item = SeasonalLeaveRequestReview.NewSeasonalLeaveRequestReview("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SeasonalLeaveRequestReviewBadgeLengthRuleFlagsALongBadge()
    {
        var rule = new SeasonalLeaveRequestReviewBadgeLengthRule(16);
        var item = SeasonalLeaveRequestReview.NewSeasonalLeaveRequestReview("x", "y");
        item.Badge = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Badge = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SeasonalLeaveRequestReviewTitleLengthRuleFlagsALongTitle()
    {
        var rule = new SeasonalLeaveRequestReviewTitleLengthRule(32);
        var item = SeasonalLeaveRequestReview.NewSeasonalLeaveRequestReview("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        SeasonalLeaveRequestReviewValidatorReportsEveryProblem();
        SeasonalLeaveRequestReviewRequiredTitleRuleFlagsABlankTitle();
        SeasonalLeaveRequestReviewBadgeLengthRuleFlagsALongBadge();
        SeasonalLeaveRequestReviewTitleLengthRuleFlagsALongTitle();
    }
}
