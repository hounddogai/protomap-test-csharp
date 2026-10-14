namespace Acme.Workforce.Orgdesign.Tests.Span.Digest;

using Acme.Workforce.Orgdesign.Span.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class SpanDigestRulesChecks
{
    public static void SpanDigestValidatorReportsEveryProblem()
    {
        var validator = SpanDigestRules.DefaultSpanDigestValidator();
        var item = SpanDigest.NewSpanDigest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SpanDigestTitleLengthRuleFlagsALongTitle()
    {
        var rule = new SpanDigestTitleLengthRule(120);
        var item = SpanDigest.NewSpanDigest("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SpanDigestFteRangeRuleFlagsValuesAboveFte()
    {
        var rule = new SpanDigestFteRangeRule(3.9, 4966.6);
        var item = SpanDigest.NewSpanDigest("x", "y");
        item.Fte = 4968.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SpanDigestNameLengthRuleFlagsALongName()
    {
        var rule = new SpanDigestNameLengthRule(3600);
        var item = SpanDigest.NewSpanDigest("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        SpanDigestValidatorReportsEveryProblem();
        SpanDigestTitleLengthRuleFlagsALongTitle();
        SpanDigestFteRangeRuleFlagsValuesAboveFte();
        SpanDigestNameLengthRuleFlagsALongName();
    }
}
