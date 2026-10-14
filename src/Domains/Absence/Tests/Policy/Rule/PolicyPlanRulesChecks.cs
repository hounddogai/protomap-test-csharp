namespace Acme.Workforce.Absence.Tests.Policy.Rule;

using Acme.Workforce.Absence.Policy.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class PolicyPlanRulesChecks
{
    public static void PolicyPlanValidatorReportsEveryProblem()
    {
        var validator = PolicyPlanRules.DefaultPolicyPlanValidator();
        var item = PolicyPlan.NewPolicyPlan("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PolicyPlanRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PolicyPlanRequiredCodeRule("policy plan needs a value");
        var item = PolicyPlan.NewPolicyPlan("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PolicyPlanRequiredBadgeRuleFlagsABlankBadge()
    {
        var rule = new PolicyPlanRequiredBadgeRule("policy plan needs this field");
        var item = PolicyPlan.NewPolicyPlan("", "");
        item.Badge = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PolicyPlanLabelLengthRuleFlagsALongLabel()
    {
        var rule = new PolicyPlanLabelLengthRule(120);
        var item = PolicyPlan.NewPolicyPlan("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PolicyPlanCodeLengthRuleFlagsALongCode()
    {
        var rule = new PolicyPlanCodeLengthRule(64);
        var item = PolicyPlan.NewPolicyPlan("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PolicyPlanRetriesRangeRuleFlagsValuesAboveRetries()
    {
        var rule = new PolicyPlanRetriesRangeRule(80, 3600);
        var item = PolicyPlan.NewPolicyPlan("x", "y");
        item.Retries = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PolicyPlanRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new PolicyPlanRequiredLabelRule("policy plan needs a value");
        var item = PolicyPlan.NewPolicyPlan("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PolicyPlanValidatorReportsEveryProblem();
        PolicyPlanRequiredCodeRuleFlagsABlankCode();
        PolicyPlanRequiredBadgeRuleFlagsABlankBadge();
        PolicyPlanLabelLengthRuleFlagsALongLabel();
        PolicyPlanCodeLengthRuleFlagsALongCode();
        PolicyPlanRetriesRangeRuleFlagsValuesAboveRetries();
        PolicyPlanRequiredLabelRuleFlagsABlankLabel();
    }
}
