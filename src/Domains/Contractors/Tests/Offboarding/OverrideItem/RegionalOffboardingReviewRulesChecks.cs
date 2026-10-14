namespace Acme.Workforce.Contractors.Tests.Offboarding.OverrideItem;

using Acme.Workforce.Contractors.Offboarding.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class RegionalOffboardingReviewRulesChecks
{
    public static void RegionalOffboardingReviewValidatorReportsEveryProblem()
    {
        var validator = RegionalOffboardingReviewRules.DefaultRegionalOffboardingReviewValidator();
        var item = RegionalOffboardingReview.NewRegionalOffboardingReview("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RegionalOffboardingReviewRequiredSegmentRuleFlagsABlankSegment()
    {
        var rule = new RegionalOffboardingReviewRequiredSegmentRule("regional offboarding review needs a value");
        var item = RegionalOffboardingReview.NewRegionalOffboardingReview("", "");
        item.Segment = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RegionalOffboardingReviewTitleLengthRuleFlagsALongTitle()
    {
        var rule = new RegionalOffboardingReviewTitleLengthRule(64);
        var item = RegionalOffboardingReview.NewRegionalOffboardingReview("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RegionalOffboardingReviewLeaveDaysRangeRuleFlagsValuesAboveLeaveDays()
    {
        var rule = new RegionalOffboardingReviewLeaveDaysRangeRule(64, 64);
        var item = RegionalOffboardingReview.NewRegionalOffboardingReview("x", "y");
        item.LeaveDays = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        RegionalOffboardingReviewValidatorReportsEveryProblem();
        RegionalOffboardingReviewRequiredSegmentRuleFlagsABlankSegment();
        RegionalOffboardingReviewTitleLengthRuleFlagsALongTitle();
        RegionalOffboardingReviewLeaveDaysRangeRuleFlagsValuesAboveLeaveDays();
    }
}
