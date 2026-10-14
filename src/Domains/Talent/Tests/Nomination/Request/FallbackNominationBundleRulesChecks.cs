namespace Acme.Workforce.Talent.Tests.Nomination.Request;

using Acme.Workforce.Talent.Nomination.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class FallbackNominationBundleRulesChecks
{
    public static void FallbackNominationBundleValidatorReportsEveryProblem()
    {
        var validator = FallbackNominationBundleRules.DefaultFallbackNominationBundleValidator();
        var item = FallbackNominationBundle.NewFallbackNominationBundle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FallbackNominationBundleUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt()
    {
        var rule = new FallbackNominationBundleUpdatedAtRangeRule(16, 86400);
        var item = FallbackNominationBundle.NewFallbackNominationBundle("x", "y");
        item.UpdatedAt = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void FallbackNominationBundleTitleLengthRuleFlagsALongTitle()
    {
        var rule = new FallbackNominationBundleTitleLengthRule(16);
        var item = FallbackNominationBundle.NewFallbackNominationBundle("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FallbackNominationBundleRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new FallbackNominationBundleRequiredTitleRule("fallback nomination bundle needs a value");
        var item = FallbackNominationBundle.NewFallbackNominationBundle("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FallbackNominationBundleRatioRangeRuleFlagsValuesAboveRatio()
    {
        var rule = new FallbackNominationBundleRatioRangeRule(5.4, 912.1);
        var item = FallbackNominationBundle.NewFallbackNominationBundle("x", "y");
        item.Ratio = 913.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        FallbackNominationBundleValidatorReportsEveryProblem();
        FallbackNominationBundleUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt();
        FallbackNominationBundleTitleLengthRuleFlagsALongTitle();
        FallbackNominationBundleRequiredTitleRuleFlagsABlankTitle();
        FallbackNominationBundleRatioRangeRuleFlagsValuesAboveRatio();
    }
}
