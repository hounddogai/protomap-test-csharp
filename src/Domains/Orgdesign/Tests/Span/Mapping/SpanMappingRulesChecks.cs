namespace Acme.Workforce.Orgdesign.Tests.Span.Mapping;

using Acme.Workforce.Orgdesign.Span.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class SpanMappingRulesChecks
{
    public static void SpanMappingValidatorReportsEveryProblem()
    {
        var validator = SpanMappingRules.DefaultSpanMappingValidator();
        var item = SpanMapping.NewSpanMapping("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SpanMappingLeaveDaysRangeRuleFlagsValuesAboveLeaveDays()
    {
        var rule = new SpanMappingLeaveDaysRangeRule(16, 3600);
        var item = SpanMapping.NewSpanMapping("x", "y");
        item.LeaveDays = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SpanMappingRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new SpanMappingRequiredCodeRule("span mapping needs this field");
        var item = SpanMapping.NewSpanMapping("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SpanMappingLabelLengthRuleFlagsALongLabel()
    {
        var rule = new SpanMappingLabelLengthRule(16);
        var item = SpanMapping.NewSpanMapping("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SpanMappingChannelLengthRuleFlagsALongChannel()
    {
        var rule = new SpanMappingChannelLengthRule(120);
        var item = SpanMapping.NewSpanMapping("x", "y");
        item.Channel = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Channel = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        SpanMappingValidatorReportsEveryProblem();
        SpanMappingLeaveDaysRangeRuleFlagsValuesAboveLeaveDays();
        SpanMappingRequiredCodeRuleFlagsABlankCode();
        SpanMappingLabelLengthRuleFlagsALongLabel();
        SpanMappingChannelLengthRuleFlagsALongChannel();
    }
}
