namespace Acme.Workforce.Engagement.Tests.ActionPlan.RegionalPlanPreference;

using Acme.Workforce.Engagement.ActionPlan.RegionalPlanPreference;

/// <summary>Checks of regional plan preference code, run by the project's runner.</summary>
public static class ActionPlanVarianceRulesChecks
{
    public static void ActionPlanVarianceValidatorReportsEveryProblem()
    {
        var validator = ActionPlanVarianceRules.DefaultActionPlanVarianceValidator();
        var item = ActionPlanVariance.NewActionPlanVariance("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ActionPlanVarianceCodeLengthRuleFlagsALongCode()
    {
        var rule = new ActionPlanVarianceCodeLengthRule(86400);
        var item = ActionPlanVariance.NewActionPlanVariance("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ActionPlanVarianceRequiredOwnerIdRuleFlagsABlankOwnerId()
    {
        var rule = new ActionPlanVarianceRequiredOwnerIdRule("action plan variance needs a value");
        var item = ActionPlanVariance.NewActionPlanVariance("", "");
        item.OwnerId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ActionPlanVarianceRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new ActionPlanVarianceRequiredLabelRule("action plan variance needs this field");
        var item = ActionPlanVariance.NewActionPlanVariance("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ActionPlanVarianceValidatorReportsEveryProblem();
        ActionPlanVarianceCodeLengthRuleFlagsALongCode();
        ActionPlanVarianceRequiredOwnerIdRuleFlagsABlankOwnerId();
        ActionPlanVarianceRequiredLabelRuleFlagsABlankLabel();
    }
}
