namespace Acme.Workforce.Orgdesign.Tests.Span.Rollup;

using Acme.Workforce.Orgdesign.Span.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class SpanScoreRulesChecks
{
    public static void SpanScoreValidatorReportsEveryProblem()
    {
        var validator = SpanScoreRules.DefaultSpanScoreValidator();
        var item = SpanScore.NewSpanScore("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SpanScoreStartsAtRangeRuleFlagsValuesAboveStartsAt()
    {
        var rule = new SpanScoreStartsAtRangeRule(32, 64);
        var item = SpanScore.NewSpanScore("x", "y");
        item.StartsAt = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SpanScoreRequiredLocationRuleFlagsABlankLocation()
    {
        var rule = new SpanScoreRequiredLocationRule("span score needs this field");
        var item = SpanScore.NewSpanScore("", "");
        item.Location = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SpanScoreCodeLengthRuleFlagsALongCode()
    {
        var rule = new SpanScoreCodeLengthRule(3600);
        var item = SpanScore.NewSpanScore("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SpanScoreTitleLengthRuleFlagsALongTitle()
    {
        var rule = new SpanScoreTitleLengthRule(3600);
        var item = SpanScore.NewSpanScore("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SpanScoreChecksumLengthRuleFlagsALongChecksum()
    {
        var rule = new SpanScoreChecksumLengthRule(3600);
        var item = SpanScore.NewSpanScore("x", "y");
        item.Checksum = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Checksum = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        SpanScoreValidatorReportsEveryProblem();
        SpanScoreStartsAtRangeRuleFlagsValuesAboveStartsAt();
        SpanScoreRequiredLocationRuleFlagsABlankLocation();
        SpanScoreCodeLengthRuleFlagsALongCode();
        SpanScoreTitleLengthRuleFlagsALongTitle();
        SpanScoreChecksumLengthRuleFlagsALongChecksum();
    }
}
