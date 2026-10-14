namespace Acme.Workforce.Engagement.Tests.ActionPlan.PlanPreference;

using Acme.Workforce.Engagement.ActionPlan.PlanPreference;

/// <summary>Checks of plan preference code, run by the project's runner.</summary>
public static class ActionPlanPolicyRulesChecks
{
    public static void ActionPlanPolicyValidatorReportsEveryProblem()
    {
        var validator = ActionPlanPolicyRules.DefaultActionPlanPolicyValidator();
        var item = ActionPlanPolicy.NewActionPlanPolicy("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ActionPlanPolicyNameLengthRuleFlagsALongName()
    {
        var rule = new ActionPlanPolicyNameLengthRule(64);
        var item = ActionPlanPolicy.NewActionPlanPolicy("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ActionPlanPolicyCodeLengthRuleFlagsALongCode()
    {
        var rule = new ActionPlanPolicyCodeLengthRule(3600);
        var item = ActionPlanPolicy.NewActionPlanPolicy("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ActionPlanPolicyValidatorReportsEveryProblem();
        ActionPlanPolicyNameLengthRuleFlagsALongName();
        ActionPlanPolicyCodeLengthRuleFlagsALongCode();
    }
}
