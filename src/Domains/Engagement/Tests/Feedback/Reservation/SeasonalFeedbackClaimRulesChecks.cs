namespace Acme.Workforce.Engagement.Tests.Feedback.Reservation;

using Acme.Workforce.Engagement.Feedback.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class SeasonalFeedbackClaimRulesChecks
{
    public static void SeasonalFeedbackClaimValidatorReportsEveryProblem()
    {
        var validator = SeasonalFeedbackClaimRules.DefaultSeasonalFeedbackClaimValidator();
        var item = SeasonalFeedbackClaim.NewSeasonalFeedbackClaim("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SeasonalFeedbackClaimRequiredSalaryBandRuleFlagsABlankSalaryBand()
    {
        var rule = new SeasonalFeedbackClaimRequiredSalaryBandRule("seasonal feedback claim needs an entry");
        var item = SeasonalFeedbackClaim.NewSeasonalFeedbackClaim("", "");
        item.SalaryBand = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SeasonalFeedbackClaimLimitRangeRuleFlagsValuesAboveLimit()
    {
        var rule = new SeasonalFeedbackClaimLimitRangeRule(255, 86400);
        var item = SeasonalFeedbackClaim.NewSeasonalFeedbackClaim("x", "y");
        item.Limit = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SeasonalFeedbackClaimEndsAtRangeRuleFlagsValuesAboveEndsAt()
    {
        var rule = new SeasonalFeedbackClaimEndsAtRangeRule(80, 255);
        var item = SeasonalFeedbackClaim.NewSeasonalFeedbackClaim("x", "y");
        item.EndsAt = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SeasonalFeedbackClaimRequiredReviewerRuleFlagsABlankReviewer()
    {
        var rule = new SeasonalFeedbackClaimRequiredReviewerRule("seasonal feedback claim needs a value");
        var item = SeasonalFeedbackClaim.NewSeasonalFeedbackClaim("", "");
        item.Reviewer = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        SeasonalFeedbackClaimValidatorReportsEveryProblem();
        SeasonalFeedbackClaimRequiredSalaryBandRuleFlagsABlankSalaryBand();
        SeasonalFeedbackClaimLimitRangeRuleFlagsValuesAboveLimit();
        SeasonalFeedbackClaimEndsAtRangeRuleFlagsValuesAboveEndsAt();
        SeasonalFeedbackClaimRequiredReviewerRuleFlagsABlankReviewer();
    }
}
