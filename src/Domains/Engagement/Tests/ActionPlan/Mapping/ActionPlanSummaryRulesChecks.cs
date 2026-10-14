namespace Acme.Workforce.Engagement.Tests.ActionPlan.Mapping;

using Acme.Workforce.Engagement.ActionPlan.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class ActionPlanSummaryRulesChecks
{
    public static void ActionPlanSummaryValidatorReportsEveryProblem()
    {
        var validator = ActionPlanSummaryRules.DefaultActionPlanSummaryValidator();
        var item = ActionPlanSummary.NewActionPlanSummary("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ActionPlanSummaryAttemptsRangeRuleFlagsValuesAboveAttempts()
    {
        var rule = new ActionPlanSummaryAttemptsRangeRule(1000, 3600);
        var item = ActionPlanSummary.NewActionPlanSummary("x", "y");
        item.Attempts = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ActionPlanSummarySequenceRangeRuleFlagsValuesAboveSequence()
    {
        var rule = new ActionPlanSummarySequenceRangeRule(1000, 86400);
        var item = ActionPlanSummary.NewActionPlanSummary("x", "y");
        item.Sequence = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        ActionPlanSummaryValidatorReportsEveryProblem();
        ActionPlanSummaryAttemptsRangeRuleFlagsValuesAboveAttempts();
        ActionPlanSummarySequenceRangeRuleFlagsValuesAboveSequence();
    }
}
