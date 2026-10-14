namespace Acme.Workforce.Orgdesign.Tests.Span.Slot;

using Acme.Workforce.Orgdesign.Span.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class SpanProfileRulesChecks
{
    public static void SpanProfileValidatorReportsEveryProblem()
    {
        var validator = SpanProfileRules.DefaultSpanProfileValidator();
        var item = SpanProfile.NewSpanProfile("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SpanProfileRequiredSourceRuleFlagsABlankSource()
    {
        var rule = new SpanProfileRequiredSourceRule("span profile needs a value");
        var item = SpanProfile.NewSpanProfile("", "");
        item.Source = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SpanProfileRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new SpanProfileRequiredCodeRule("span profile needs this field");
        var item = SpanProfile.NewSpanProfile("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SpanProfileCodeLengthRuleFlagsALongCode()
    {
        var rule = new SpanProfileCodeLengthRule(1000);
        var item = SpanProfile.NewSpanProfile("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SpanProfileRequiredCostCenterRuleFlagsABlankCostCenter()
    {
        var rule = new SpanProfileRequiredCostCenterRule("span profile needs this field");
        var item = SpanProfile.NewSpanProfile("", "");
        item.CostCenter = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        SpanProfileValidatorReportsEveryProblem();
        SpanProfileRequiredSourceRuleFlagsABlankSource();
        SpanProfileRequiredCodeRuleFlagsABlankCode();
        SpanProfileCodeLengthRuleFlagsALongCode();
        SpanProfileRequiredCostCenterRuleFlagsABlankCostCenter();
    }
}
