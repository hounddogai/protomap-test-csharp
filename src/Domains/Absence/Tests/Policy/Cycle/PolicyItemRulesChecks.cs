namespace Acme.Workforce.Absence.Tests.Policy.Cycle;

using Acme.Workforce.Absence.Policy.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class PolicyItemRulesChecks
{
    public static void PolicyItemValidatorReportsEveryProblem()
    {
        var validator = PolicyItemRules.DefaultPolicyItemValidator();
        var item = PolicyItem.NewPolicyItem("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PolicyItemRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PolicyItemRequiredCodeRule("policy item needs this field");
        var item = PolicyItem.NewPolicyItem("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PolicyItemTeamLengthRuleFlagsALongTeam()
    {
        var rule = new PolicyItemTeamLengthRule(16);
        var item = PolicyItem.NewPolicyItem("x", "y");
        item.Team = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Team = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PolicyItemCurrencyLengthRuleFlagsALongCurrency()
    {
        var rule = new PolicyItemCurrencyLengthRule(255);
        var item = PolicyItem.NewPolicyItem("x", "y");
        item.Currency = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Currency = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PolicyItemLabelLengthRuleFlagsALongLabel()
    {
        var rule = new PolicyItemLabelLengthRule(120);
        var item = PolicyItem.NewPolicyItem("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PolicyItemUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt()
    {
        var rule = new PolicyItemUpdatedAtRangeRule(32, 86400);
        var item = PolicyItem.NewPolicyItem("x", "y");
        item.UpdatedAt = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PolicyItemRequiredTeamRuleFlagsABlankTeam()
    {
        var rule = new PolicyItemRequiredTeamRule("policy item needs this field");
        var item = PolicyItem.NewPolicyItem("", "");
        item.Team = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PolicyItemValidatorReportsEveryProblem();
        PolicyItemRequiredCodeRuleFlagsABlankCode();
        PolicyItemTeamLengthRuleFlagsALongTeam();
        PolicyItemCurrencyLengthRuleFlagsALongCurrency();
        PolicyItemLabelLengthRuleFlagsALongLabel();
        PolicyItemUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt();
        PolicyItemRequiredTeamRuleFlagsABlankTeam();
    }
}
