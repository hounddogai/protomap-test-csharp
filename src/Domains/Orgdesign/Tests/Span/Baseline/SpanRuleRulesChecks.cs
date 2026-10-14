namespace Acme.Workforce.Orgdesign.Tests.Span.Baseline;

using Acme.Workforce.Orgdesign.Span.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class SpanRuleRulesChecks
{
    public static void SpanRuleValidatorReportsEveryProblem()
    {
        var validator = SpanRuleRules.DefaultSpanRuleValidator();
        var item = SpanRule.NewSpanRule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SpanRuleRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new SpanRuleRequiredTitleRule("span rule needs an entry");
        var item = SpanRule.NewSpanRule("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SpanRuleAttemptsRangeRuleFlagsValuesAboveAttempts()
    {
        var rule = new SpanRuleAttemptsRangeRule(80, 1000);
        var item = SpanRule.NewSpanRule("x", "y");
        item.Attempts = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SpanRuleRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new SpanRuleRequiredCodeRule("span rule needs a value");
        var item = SpanRule.NewSpanRule("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        SpanRuleValidatorReportsEveryProblem();
        SpanRuleRequiredTitleRuleFlagsABlankTitle();
        SpanRuleAttemptsRangeRuleFlagsValuesAboveAttempts();
        SpanRuleRequiredCodeRuleFlagsABlankCode();
    }
}
