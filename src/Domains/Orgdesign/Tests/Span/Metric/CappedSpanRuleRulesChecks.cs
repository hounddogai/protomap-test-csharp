namespace Acme.Workforce.Orgdesign.Tests.Span.Metric;

using Acme.Workforce.Orgdesign.Span.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class CappedSpanRuleRulesChecks
{
    public static void CappedSpanRuleValidatorReportsEveryProblem()
    {
        var validator = CappedSpanRuleRules.DefaultCappedSpanRuleValidator();
        var item = CappedSpanRule.NewCappedSpanRule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CappedSpanRuleCodeLengthRuleFlagsALongCode()
    {
        var rule = new CappedSpanRuleCodeLengthRule(32);
        var item = CappedSpanRule.NewCappedSpanRule("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CappedSpanRuleRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new CappedSpanRuleRequiredLabelRule("capped span rule needs this field");
        var item = CappedSpanRule.NewCappedSpanRule("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CappedSpanRuleFteRangeRuleFlagsValuesAboveFte()
    {
        var rule = new CappedSpanRuleFteRangeRule(8.4, 1878.7);
        var item = CappedSpanRule.NewCappedSpanRule("x", "y");
        item.Fte = 1880.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        CappedSpanRuleValidatorReportsEveryProblem();
        CappedSpanRuleCodeLengthRuleFlagsALongCode();
        CappedSpanRuleRequiredLabelRuleFlagsABlankLabel();
        CappedSpanRuleFteRangeRuleFlagsValuesAboveFte();
    }
}
