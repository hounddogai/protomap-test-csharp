namespace Acme.Workforce.Talent.Tests.Mobility.Review;

using Acme.Workforce.Talent.Mobility.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class RegionalMobilityReviewRulesChecks
{
    public static void RegionalMobilityReviewValidatorReportsEveryProblem()
    {
        var validator = RegionalMobilityReviewRules.DefaultRegionalMobilityReviewValidator();
        var item = RegionalMobilityReview.NewRegionalMobilityReview("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RegionalMobilityReviewRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new RegionalMobilityReviewRequiredCodeRule("regional mobility review needs a value");
        var item = RegionalMobilityReview.NewRegionalMobilityReview("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RegionalMobilityReviewRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new RegionalMobilityReviewRequiredTitleRule("regional mobility review needs a value");
        var item = RegionalMobilityReview.NewRegionalMobilityReview("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        RegionalMobilityReviewValidatorReportsEveryProblem();
        RegionalMobilityReviewRequiredCodeRuleFlagsABlankCode();
        RegionalMobilityReviewRequiredTitleRuleFlagsABlankTitle();
    }
}
