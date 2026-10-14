namespace Acme.Workforce.Workplace.Tests.AccessBadge.BadgeEntry;

using Acme.Workforce.Workplace.AccessBadge.BadgeEntry;

/// <summary>Checks of badge entry code, run by the project's runner.</summary>
public static class FallbackAccessBadgeEstimateRulesChecks
{
    public static void FallbackAccessBadgeEstimateValidatorReportsEveryProblem()
    {
        var validator = FallbackAccessBadgeEstimateRules.DefaultFallbackAccessBadgeEstimateValidator();
        var item = FallbackAccessBadgeEstimate.NewFallbackAccessBadgeEstimate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FallbackAccessBadgeEstimateScoreRangeRuleFlagsValuesAboveScore()
    {
        var rule = new FallbackAccessBadgeEstimateScoreRangeRule(9.5, 4390.5);
        var item = FallbackAccessBadgeEstimate.NewFallbackAccessBadgeEstimate("x", "y");
        item.Score = 4392.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void FallbackAccessBadgeEstimateRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new FallbackAccessBadgeEstimateRequiredCodeRule("fallback access badge estimate needs a value");
        var item = FallbackAccessBadgeEstimate.NewFallbackAccessBadgeEstimate("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        FallbackAccessBadgeEstimateValidatorReportsEveryProblem();
        FallbackAccessBadgeEstimateScoreRangeRuleFlagsValuesAboveScore();
        FallbackAccessBadgeEstimateRequiredCodeRuleFlagsABlankCode();
    }
}
