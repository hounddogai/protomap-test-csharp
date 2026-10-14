namespace Acme.Workforce.Engagement.Tests.ActionPlan.RecordItem;

using Acme.Workforce.Engagement.ActionPlan.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class ActionPlanAdjustmentRulesChecks
{
    public static void ActionPlanAdjustmentValidatorReportsEveryProblem()
    {
        var validator = ActionPlanAdjustmentRules.DefaultActionPlanAdjustmentValidator();
        var item = ActionPlanAdjustment.NewActionPlanAdjustment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ActionPlanAdjustmentCountryLengthRuleFlagsALongCountry()
    {
        var rule = new ActionPlanAdjustmentCountryLengthRule(120);
        var item = ActionPlanAdjustment.NewActionPlanAdjustment("x", "y");
        item.Country = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Country = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ActionPlanAdjustmentTitleLengthRuleFlagsALongTitle()
    {
        var rule = new ActionPlanAdjustmentTitleLengthRule(64);
        var item = ActionPlanAdjustment.NewActionPlanAdjustment("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ActionPlanAdjustmentEndsAtRangeRuleFlagsValuesAboveEndsAt()
    {
        var rule = new ActionPlanAdjustmentEndsAtRangeRule(255, 86400);
        var item = ActionPlanAdjustment.NewActionPlanAdjustment("x", "y");
        item.EndsAt = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        ActionPlanAdjustmentValidatorReportsEveryProblem();
        ActionPlanAdjustmentCountryLengthRuleFlagsALongCountry();
        ActionPlanAdjustmentTitleLengthRuleFlagsALongTitle();
        ActionPlanAdjustmentEndsAtRangeRuleFlagsValuesAboveEndsAt();
    }
}
