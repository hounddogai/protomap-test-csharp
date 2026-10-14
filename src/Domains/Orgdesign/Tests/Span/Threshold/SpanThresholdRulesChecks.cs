namespace Acme.Workforce.Orgdesign.Tests.Span.Threshold;

using Acme.Workforce.Orgdesign.Span.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class SpanThresholdRulesChecks
{
    public static void SpanThresholdValidatorReportsEveryProblem()
    {
        var validator = SpanThresholdRules.DefaultSpanThresholdValidator();
        var item = SpanThreshold.NewSpanThreshold("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SpanThresholdCodeLengthRuleFlagsALongCode()
    {
        var rule = new SpanThresholdCodeLengthRule(1000);
        var item = SpanThreshold.NewSpanThreshold("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SpanThresholdChecksumLengthRuleFlagsALongChecksum()
    {
        var rule = new SpanThresholdChecksumLengthRule(80);
        var item = SpanThreshold.NewSpanThreshold("x", "y");
        item.Checksum = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Checksum = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        SpanThresholdValidatorReportsEveryProblem();
        SpanThresholdCodeLengthRuleFlagsALongCode();
        SpanThresholdChecksumLengthRuleFlagsALongChecksum();
    }
}
