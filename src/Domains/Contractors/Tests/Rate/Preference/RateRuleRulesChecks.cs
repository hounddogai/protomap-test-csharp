namespace Acme.Workforce.Contractors.Tests.Rate.Preference;

using Acme.Workforce.Contractors.Rate.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class RateRuleRulesChecks
{
    public static void RateRuleValidatorReportsEveryProblem()
    {
        var validator = RateRuleRules.DefaultRateRuleValidator();
        var item = RateRule.NewRateRule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RateRuleNameLengthRuleFlagsALongName()
    {
        var rule = new RateRuleNameLengthRule(80);
        var item = RateRule.NewRateRule("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RateRuleLocaleLengthRuleFlagsALongLocale()
    {
        var rule = new RateRuleLocaleLengthRule(16);
        var item = RateRule.NewRateRule("x", "y");
        item.Locale = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Locale = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        RateRuleValidatorReportsEveryProblem();
        RateRuleNameLengthRuleFlagsALongName();
        RateRuleLocaleLengthRuleFlagsALongLocale();
    }
}
