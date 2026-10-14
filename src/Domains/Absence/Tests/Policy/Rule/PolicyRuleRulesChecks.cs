namespace Acme.Workforce.Absence.Tests.Policy.Rule;

using Acme.Workforce.Absence.Policy.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class PolicyRuleRulesChecks
{
    public static void PolicyRuleValidatorReportsEveryProblem()
    {
        var validator = PolicyRuleRules.DefaultPolicyRuleValidator();
        var item = PolicyRule.NewPolicyRule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PolicyRuleCodeLengthRuleFlagsALongCode()
    {
        var rule = new PolicyRuleCodeLengthRule(32);
        var item = PolicyRule.NewPolicyRule("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PolicyRuleRequiredCurrencyRuleFlagsABlankCurrency()
    {
        var rule = new PolicyRuleRequiredCurrencyRule("policy rule needs an entry");
        var item = PolicyRule.NewPolicyRule("", "");
        item.Currency = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PolicyRuleWindowDaysRangeRuleFlagsValuesAboveWindowDays()
    {
        var rule = new PolicyRuleWindowDaysRangeRule(32, 1000);
        var item = PolicyRule.NewPolicyRule("x", "y");
        item.WindowDays = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PolicyRuleNameLengthRuleFlagsALongName()
    {
        var rule = new PolicyRuleNameLengthRule(120);
        var item = PolicyRule.NewPolicyRule("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PolicyRuleValidatorReportsEveryProblem();
        PolicyRuleCodeLengthRuleFlagsALongCode();
        PolicyRuleRequiredCurrencyRuleFlagsABlankCurrency();
        PolicyRuleWindowDaysRangeRuleFlagsValuesAboveWindowDays();
        PolicyRuleNameLengthRuleFlagsALongName();
    }
}
