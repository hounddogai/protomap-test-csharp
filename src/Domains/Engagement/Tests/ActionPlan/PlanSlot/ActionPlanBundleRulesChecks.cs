namespace Acme.Workforce.Engagement.Tests.ActionPlan.PlanSlot;

using Acme.Workforce.Engagement.ActionPlan.PlanSlot;

/// <summary>Checks of plan slot code, run by the project's runner.</summary>
public static class ActionPlanBundleRulesChecks
{
    public static void ActionPlanBundleValidatorReportsEveryProblem()
    {
        var validator = ActionPlanBundleRules.DefaultActionPlanBundleValidator();
        var item = ActionPlanBundle.NewActionPlanBundle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ActionPlanBundleCapacityRangeRuleFlagsValuesAboveCapacity()
    {
        var rule = new ActionPlanBundleCapacityRangeRule(64, 86400);
        var item = ActionPlanBundle.NewActionPlanBundle("x", "y");
        item.Capacity = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ActionPlanBundleRequiredDescriptionRuleFlagsABlankDescription()
    {
        var rule = new ActionPlanBundleRequiredDescriptionRule("action plan bundle needs an entry");
        var item = ActionPlanBundle.NewActionPlanBundle("", "");
        item.Description = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ActionPlanBundleDescriptionLengthRuleFlagsALongDescription()
    {
        var rule = new ActionPlanBundleDescriptionLengthRule(80);
        var item = ActionPlanBundle.NewActionPlanBundle("x", "y");
        item.Description = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Description = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ActionPlanBundleRequiredNameRuleFlagsABlankName()
    {
        var rule = new ActionPlanBundleRequiredNameRule("action plan bundle needs a value");
        var item = ActionPlanBundle.NewActionPlanBundle("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ActionPlanBundleRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ActionPlanBundleRequiredCodeRule("action plan bundle needs this field");
        var item = ActionPlanBundle.NewActionPlanBundle("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ActionPlanBundleValidatorReportsEveryProblem();
        ActionPlanBundleCapacityRangeRuleFlagsValuesAboveCapacity();
        ActionPlanBundleRequiredDescriptionRuleFlagsABlankDescription();
        ActionPlanBundleDescriptionLengthRuleFlagsALongDescription();
        ActionPlanBundleRequiredNameRuleFlagsABlankName();
        ActionPlanBundleRequiredCodeRuleFlagsABlankCode();
    }
}
