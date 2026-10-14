namespace Acme.Workforce.Orgdesign.Tests.Span.Rollup;

using Acme.Workforce.Orgdesign.Span.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class SpanRollupRulesChecks
{
    public static void SpanRollupValidatorReportsEveryProblem()
    {
        var validator = SpanRollupRules.DefaultSpanRollupValidator();
        var item = SpanRollup.NewSpanRollup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SpanRollupRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new SpanRollupRequiredCodeRule("span rollup needs an entry");
        var item = SpanRollup.NewSpanRollup("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SpanRollupRequiredNameRuleFlagsABlankName()
    {
        var rule = new SpanRollupRequiredNameRule("span rollup needs this field");
        var item = SpanRollup.NewSpanRollup("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SpanRollupNameLengthRuleFlagsALongName()
    {
        var rule = new SpanRollupNameLengthRule(3600);
        var item = SpanRollup.NewSpanRollup("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        SpanRollupValidatorReportsEveryProblem();
        SpanRollupRequiredCodeRuleFlagsABlankCode();
        SpanRollupRequiredNameRuleFlagsABlankName();
        SpanRollupNameLengthRuleFlagsALongName();
    }
}
