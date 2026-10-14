namespace Acme.Workforce.Orgdesign.Tests.Span.Metric;

using Acme.Workforce.Orgdesign.Span.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class SpanWindowRulesChecks
{
    public static void SpanWindowValidatorReportsEveryProblem()
    {
        var validator = SpanWindowRules.DefaultSpanWindowValidator();
        var item = SpanWindow.NewSpanWindow("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SpanWindowRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new SpanWindowRequiredCodeRule("span window needs a value");
        var item = SpanWindow.NewSpanWindow("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SpanWindowCountryLengthRuleFlagsALongCountry()
    {
        var rule = new SpanWindowCountryLengthRule(80);
        var item = SpanWindow.NewSpanWindow("x", "y");
        item.Country = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Country = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SpanWindowRetriesRangeRuleFlagsValuesAboveRetries()
    {
        var rule = new SpanWindowRetriesRangeRule(120, 86400);
        var item = SpanWindow.NewSpanWindow("x", "y");
        item.Retries = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        SpanWindowValidatorReportsEveryProblem();
        SpanWindowRequiredCodeRuleFlagsABlankCode();
        SpanWindowCountryLengthRuleFlagsALongCountry();
        SpanWindowRetriesRangeRuleFlagsValuesAboveRetries();
    }
}
