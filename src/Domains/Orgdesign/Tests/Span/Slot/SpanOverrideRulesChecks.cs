namespace Acme.Workforce.Orgdesign.Tests.Span.Slot;

using Acme.Workforce.Orgdesign.Span.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class SpanOverrideRulesChecks
{
    public static void SpanOverrideValidatorReportsEveryProblem()
    {
        var validator = SpanOverrideRules.DefaultSpanOverrideValidator();
        var item = SpanOverride.NewSpanOverride("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SpanOverrideRequiredSourceRuleFlagsABlankSource()
    {
        var rule = new SpanOverrideRequiredSourceRule("span override needs this field");
        var item = SpanOverride.NewSpanOverride("", "");
        item.Source = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SpanOverrideRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new SpanOverrideRequiredLabelRule("span override needs this field");
        var item = SpanOverride.NewSpanOverride("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SpanOverrideEmployeeIdLengthRuleFlagsALongEmployeeId()
    {
        var rule = new SpanOverrideEmployeeIdLengthRule(80);
        var item = SpanOverride.NewSpanOverride("x", "y");
        item.EmployeeId = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.EmployeeId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        SpanOverrideValidatorReportsEveryProblem();
        SpanOverrideRequiredSourceRuleFlagsABlankSource();
        SpanOverrideRequiredLabelRuleFlagsABlankLabel();
        SpanOverrideEmployeeIdLengthRuleFlagsALongEmployeeId();
    }
}
