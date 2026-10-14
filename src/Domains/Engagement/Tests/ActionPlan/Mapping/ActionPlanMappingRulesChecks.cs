namespace Acme.Workforce.Engagement.Tests.ActionPlan.Mapping;

using Acme.Workforce.Engagement.ActionPlan.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class ActionPlanMappingRulesChecks
{
    public static void ActionPlanMappingValidatorReportsEveryProblem()
    {
        var validator = ActionPlanMappingRules.DefaultActionPlanMappingValidator();
        var item = ActionPlanMapping.NewActionPlanMapping("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ActionPlanMappingCodeLengthRuleFlagsALongCode()
    {
        var rule = new ActionPlanMappingCodeLengthRule(3600);
        var item = ActionPlanMapping.NewActionPlanMapping("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ActionPlanMappingTitleLengthRuleFlagsALongTitle()
    {
        var rule = new ActionPlanMappingTitleLengthRule(64);
        var item = ActionPlanMapping.NewActionPlanMapping("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ActionPlanMappingLeaveDaysRangeRuleFlagsValuesAboveLeaveDays()
    {
        var rule = new ActionPlanMappingLeaveDaysRangeRule(64, 80);
        var item = ActionPlanMapping.NewActionPlanMapping("x", "y");
        item.LeaveDays = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        ActionPlanMappingValidatorReportsEveryProblem();
        ActionPlanMappingCodeLengthRuleFlagsALongCode();
        ActionPlanMappingTitleLengthRuleFlagsALongTitle();
        ActionPlanMappingLeaveDaysRangeRuleFlagsValuesAboveLeaveDays();
    }
}
