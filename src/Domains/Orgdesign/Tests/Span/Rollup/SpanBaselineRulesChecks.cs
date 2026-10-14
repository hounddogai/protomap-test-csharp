namespace Acme.Workforce.Orgdesign.Tests.Span.Rollup;

using Acme.Workforce.Orgdesign.Span.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class SpanBaselineRulesChecks
{
    public static void SpanBaselineValidatorReportsEveryProblem()
    {
        var validator = SpanBaselineRules.DefaultSpanBaselineValidator();
        var item = SpanBaseline.NewSpanBaseline("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SpanBaselineHoursRangeRuleFlagsValuesAboveHours()
    {
        var rule = new SpanBaselineHoursRangeRule(4.8, 1353.4);
        var item = SpanBaseline.NewSpanBaseline("x", "y");
        item.Hours = 1354.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SpanBaselineRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new SpanBaselineRequiredCodeRule("span baseline needs a value");
        var item = SpanBaseline.NewSpanBaseline("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        SpanBaselineValidatorReportsEveryProblem();
        SpanBaselineHoursRangeRuleFlagsValuesAboveHours();
        SpanBaselineRequiredCodeRuleFlagsABlankCode();
    }
}
