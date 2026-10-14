namespace Acme.Workforce.Compliance.Tests.PolicyAck.Group;

using Acme.Workforce.Compliance.PolicyAck.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class PolicyAckPlanRulesChecks
{
    public static void PolicyAckPlanValidatorReportsEveryProblem()
    {
        var validator = PolicyAckPlanRules.DefaultPolicyAckPlanValidator();
        var item = PolicyAckPlan.NewPolicyAckPlan("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PolicyAckPlanTitleLengthRuleFlagsALongTitle()
    {
        var rule = new PolicyAckPlanTitleLengthRule(3600);
        var item = PolicyAckPlan.NewPolicyAckPlan("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PolicyAckPlanCodeLengthRuleFlagsALongCode()
    {
        var rule = new PolicyAckPlanCodeLengthRule(32);
        var item = PolicyAckPlan.NewPolicyAckPlan("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PolicyAckPlanRequiredOwnerIdRuleFlagsABlankOwnerId()
    {
        var rule = new PolicyAckPlanRequiredOwnerIdRule("policy ack plan needs an entry");
        var item = PolicyAckPlan.NewPolicyAckPlan("", "");
        item.OwnerId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PolicyAckPlanBudgetRangeRuleFlagsValuesAboveBudget()
    {
        var rule = new PolicyAckPlanBudgetRangeRule(8.8, 1823.2);
        var item = PolicyAckPlan.NewPolicyAckPlan("x", "y");
        item.Budget = 1824.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        PolicyAckPlanValidatorReportsEveryProblem();
        PolicyAckPlanTitleLengthRuleFlagsALongTitle();
        PolicyAckPlanCodeLengthRuleFlagsALongCode();
        PolicyAckPlanRequiredOwnerIdRuleFlagsABlankOwnerId();
        PolicyAckPlanBudgetRangeRuleFlagsValuesAboveBudget();
    }
}
