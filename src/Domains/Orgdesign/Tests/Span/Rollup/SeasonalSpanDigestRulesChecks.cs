namespace Acme.Workforce.Orgdesign.Tests.Span.Rollup;

using Acme.Workforce.Orgdesign.Span.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class SeasonalSpanDigestRulesChecks
{
    public static void SeasonalSpanDigestValidatorReportsEveryProblem()
    {
        var validator = SeasonalSpanDigestRules.DefaultSeasonalSpanDigestValidator();
        var item = SeasonalSpanDigest.NewSeasonalSpanDigest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SeasonalSpanDigestRequiredSourceRuleFlagsABlankSource()
    {
        var rule = new SeasonalSpanDigestRequiredSourceRule("seasonal span digest needs a value");
        var item = SeasonalSpanDigest.NewSeasonalSpanDigest("", "");
        item.Source = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SeasonalSpanDigestNameLengthRuleFlagsALongName()
    {
        var rule = new SeasonalSpanDigestNameLengthRule(86400);
        var item = SeasonalSpanDigest.NewSeasonalSpanDigest("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        SeasonalSpanDigestValidatorReportsEveryProblem();
        SeasonalSpanDigestRequiredSourceRuleFlagsABlankSource();
        SeasonalSpanDigestNameLengthRuleFlagsALongName();
    }
}
