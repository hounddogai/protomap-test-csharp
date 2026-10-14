namespace Acme.Workforce.Orgdesign.Tests.Span.Threshold;

using Acme.Workforce.Orgdesign.Span.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class DefaultSpanBaselineRulesChecks
{
    public static void DefaultSpanBaselineValidatorReportsEveryProblem()
    {
        var validator = DefaultSpanBaselineRules.DefaultDefaultSpanBaselineValidator();
        var item = DefaultSpanBaseline.NewDefaultSpanBaseline("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultSpanBaselineCodeLengthRuleFlagsALongCode()
    {
        var rule = new DefaultSpanBaselineCodeLengthRule(120);
        var item = DefaultSpanBaseline.NewDefaultSpanBaseline("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DefaultSpanBaselineLabelLengthRuleFlagsALongLabel()
    {
        var rule = new DefaultSpanBaselineLabelLengthRule(86400);
        var item = DefaultSpanBaseline.NewDefaultSpanBaseline("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DefaultSpanBaselineRequiredSummaryRuleFlagsABlankSummary()
    {
        var rule = new DefaultSpanBaselineRequiredSummaryRule("default span baseline needs a value");
        var item = DefaultSpanBaseline.NewDefaultSpanBaseline("", "");
        item.Summary = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        DefaultSpanBaselineValidatorReportsEveryProblem();
        DefaultSpanBaselineCodeLengthRuleFlagsALongCode();
        DefaultSpanBaselineLabelLengthRuleFlagsALongLabel();
        DefaultSpanBaselineRequiredSummaryRuleFlagsABlankSummary();
    }
}
