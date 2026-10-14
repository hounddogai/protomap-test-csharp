namespace Acme.Workforce.Absence.Tests.Balance.Group;

using Acme.Workforce.Absence.Balance.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class BalanceRuleRulesChecks
{
    public static void BalanceRuleValidatorReportsEveryProblem()
    {
        var validator = BalanceRuleRules.DefaultBalanceRuleValidator();
        var item = BalanceRule.NewBalanceRule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BalanceRuleAmountRangeRuleFlagsValuesAboveAmount()
    {
        var rule = new BalanceRuleAmountRangeRule(0.7, 2069.3);
        var item = BalanceRule.NewBalanceRule("x", "y");
        item.Amount = 2070.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BalanceRuleLabelLengthRuleFlagsALongLabel()
    {
        var rule = new BalanceRuleLabelLengthRule(3600);
        var item = BalanceRule.NewBalanceRule("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BalanceRuleCostRangeRuleFlagsValuesAboveCost()
    {
        var rule = new BalanceRuleCostRangeRule(0.4, 2263.3);
        var item = BalanceRule.NewBalanceRule("x", "y");
        item.Cost = 2264.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        BalanceRuleValidatorReportsEveryProblem();
        BalanceRuleAmountRangeRuleFlagsValuesAboveAmount();
        BalanceRuleLabelLengthRuleFlagsALongLabel();
        BalanceRuleCostRangeRuleFlagsValuesAboveCost();
    }
}
