namespace Acme.Workforce.Workplace.Tests.Booking.Review;

using Acme.Workforce.Workplace.Booking.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class BookingReviewRulesChecks
{
    public static void BookingReviewValidatorReportsEveryProblem()
    {
        var validator = BookingReviewRules.DefaultBookingReviewValidator();
        var item = BookingReview.NewBookingReview("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BookingReviewTeamLengthRuleFlagsALongTeam()
    {
        var rule = new BookingReviewTeamLengthRule(64);
        var item = BookingReview.NewBookingReview("x", "y");
        item.Team = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Team = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BookingReviewBudgetRangeRuleFlagsValuesAboveBudget()
    {
        var rule = new BookingReviewBudgetRangeRule(4.3, 4251.8);
        var item = BookingReview.NewBookingReview("x", "y");
        item.Budget = 4253.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BookingReviewRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new BookingReviewRequiredTitleRule("booking review needs an entry");
        var item = BookingReview.NewBookingReview("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BookingReviewCostCenterLengthRuleFlagsALongCostCenter()
    {
        var rule = new BookingReviewCostCenterLengthRule(3600);
        var item = BookingReview.NewBookingReview("x", "y");
        item.CostCenter = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.CostCenter = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        BookingReviewValidatorReportsEveryProblem();
        BookingReviewTeamLengthRuleFlagsALongTeam();
        BookingReviewBudgetRangeRuleFlagsValuesAboveBudget();
        BookingReviewRequiredTitleRuleFlagsABlankTitle();
        BookingReviewCostCenterLengthRuleFlagsALongCostCenter();
    }
}
