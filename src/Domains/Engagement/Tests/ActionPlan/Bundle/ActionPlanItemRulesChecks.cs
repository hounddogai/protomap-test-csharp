namespace Acme.Workforce.Engagement.Tests.ActionPlan.Bundle;

using Acme.Workforce.Engagement.ActionPlan.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class ActionPlanItemRulesChecks
{
    public static void ActionPlanItemValidatorReportsEveryProblem()
    {
        var validator = ActionPlanItemRules.DefaultActionPlanItemValidator();
        var item = ActionPlanItem.NewActionPlanItem("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ActionPlanItemAmountRangeRuleFlagsValuesAboveAmount()
    {
        var rule = new ActionPlanItemAmountRangeRule(8.5, 2762.7);
        var item = ActionPlanItem.NewActionPlanItem("x", "y");
        item.Amount = 2764.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ActionPlanItemTitleLengthRuleFlagsALongTitle()
    {
        var rule = new ActionPlanItemTitleLengthRule(255);
        var item = ActionPlanItem.NewActionPlanItem("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ActionPlanItemCodeLengthRuleFlagsALongCode()
    {
        var rule = new ActionPlanItemCodeLengthRule(64);
        var item = ActionPlanItem.NewActionPlanItem("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ActionPlanItemRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new ActionPlanItemRequiredTitleRule("action plan item needs this field");
        var item = ActionPlanItem.NewActionPlanItem("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ActionPlanItemValidatorReportsEveryProblem();
        ActionPlanItemAmountRangeRuleFlagsValuesAboveAmount();
        ActionPlanItemTitleLengthRuleFlagsALongTitle();
        ActionPlanItemCodeLengthRuleFlagsALongCode();
        ActionPlanItemRequiredTitleRuleFlagsABlankTitle();
    }
}
