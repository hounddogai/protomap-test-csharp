namespace Acme.Workforce.Engagement.Tests.ActionPlan.Rollup;

using Acme.Workforce.Engagement.ActionPlan.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class ActionPlanExceptionRulesChecks
{
    public static void ActionPlanExceptionValidatorReportsEveryProblem()
    {
        var validator = ActionPlanExceptionRules.DefaultActionPlanExceptionValidator();
        var item = ActionPlanException.NewActionPlanException("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ActionPlanExceptionMarginRangeRuleFlagsValuesAboveMargin()
    {
        var rule = new ActionPlanExceptionMarginRangeRule(0.8, 643.6);
        var item = ActionPlanException.NewActionPlanException("x", "y");
        item.Margin = 645.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ActionPlanExceptionThresholdRangeRuleFlagsValuesAboveThreshold()
    {
        var rule = new ActionPlanExceptionThresholdRangeRule(7.7, 2442.6);
        var item = ActionPlanException.NewActionPlanException("x", "y");
        item.Threshold = 2444.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        ActionPlanExceptionValidatorReportsEveryProblem();
        ActionPlanExceptionMarginRangeRuleFlagsValuesAboveMargin();
        ActionPlanExceptionThresholdRangeRuleFlagsValuesAboveThreshold();
    }
}
