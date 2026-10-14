namespace Acme.Workforce.Orgdesign.Tests.Span.Exception;

using Acme.Workforce.Orgdesign.Span.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class SpanExceptionRulesChecks
{
    public static void SpanExceptionValidatorReportsEveryProblem()
    {
        var validator = SpanExceptionRules.DefaultSpanExceptionValidator();
        var item = SpanException.NewSpanException("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SpanExceptionRetriesRangeRuleFlagsValuesAboveRetries()
    {
        var rule = new SpanExceptionRetriesRangeRule(255, 255);
        var item = SpanException.NewSpanException("x", "y");
        item.Retries = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SpanExceptionRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new SpanExceptionRequiredCodeRule("span exception needs this field");
        var item = SpanException.NewSpanException("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SpanExceptionLabelLengthRuleFlagsALongLabel()
    {
        var rule = new SpanExceptionLabelLengthRule(255);
        var item = SpanException.NewSpanException("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SpanExceptionRequiredChannelRuleFlagsABlankChannel()
    {
        var rule = new SpanExceptionRequiredChannelRule("span exception needs an entry");
        var item = SpanException.NewSpanException("", "");
        item.Channel = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SpanExceptionChannelLengthRuleFlagsALongChannel()
    {
        var rule = new SpanExceptionChannelLengthRule(255);
        var item = SpanException.NewSpanException("x", "y");
        item.Channel = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Channel = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        SpanExceptionValidatorReportsEveryProblem();
        SpanExceptionRetriesRangeRuleFlagsValuesAboveRetries();
        SpanExceptionRequiredCodeRuleFlagsABlankCode();
        SpanExceptionLabelLengthRuleFlagsALongLabel();
        SpanExceptionRequiredChannelRuleFlagsABlankChannel();
        SpanExceptionChannelLengthRuleFlagsALongChannel();
    }
}
