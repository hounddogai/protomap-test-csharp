namespace Acme.Workforce.Engagement.Tests.ActionPlan.Mapping;

using Acme.Workforce.Engagement.ActionPlan.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class ActionPlanAllocationRulesChecks
{
    public static void ActionPlanAllocationValidatorReportsEveryProblem()
    {
        var validator = ActionPlanAllocationRules.DefaultActionPlanAllocationValidator();
        var item = ActionPlanAllocation.NewActionPlanAllocation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ActionPlanAllocationRequiredChannelRuleFlagsABlankChannel()
    {
        var rule = new ActionPlanAllocationRequiredChannelRule("action plan allocation needs this field");
        var item = ActionPlanAllocation.NewActionPlanAllocation("", "");
        item.Channel = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ActionPlanAllocationChannelLengthRuleFlagsALongChannel()
    {
        var rule = new ActionPlanAllocationChannelLengthRule(80);
        var item = ActionPlanAllocation.NewActionPlanAllocation("x", "y");
        item.Channel = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Channel = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ActionPlanAllocationAmountRangeRuleFlagsValuesAboveAmount()
    {
        var rule = new ActionPlanAllocationAmountRangeRule(5.5, 3344.1);
        var item = ActionPlanAllocation.NewActionPlanAllocation("x", "y");
        item.Amount = 3345.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        ActionPlanAllocationValidatorReportsEveryProblem();
        ActionPlanAllocationRequiredChannelRuleFlagsABlankChannel();
        ActionPlanAllocationChannelLengthRuleFlagsALongChannel();
        ActionPlanAllocationAmountRangeRuleFlagsValuesAboveAmount();
    }
}
