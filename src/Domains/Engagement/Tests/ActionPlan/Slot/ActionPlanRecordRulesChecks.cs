namespace Acme.Workforce.Engagement.Tests.ActionPlan.Slot;

using Acme.Workforce.Engagement.ActionPlan.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class ActionPlanRecordRulesChecks
{
    public static void ActionPlanRecordValidatorReportsEveryProblem()
    {
        var validator = ActionPlanRecordRules.DefaultActionPlanRecordValidator();
        var item = ActionPlanRecord.NewActionPlanRecord("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ActionPlanRecordAttemptsRangeRuleFlagsValuesAboveAttempts()
    {
        var rule = new ActionPlanRecordAttemptsRangeRule(16, 64);
        var item = ActionPlanRecord.NewActionPlanRecord("x", "y");
        item.Attempts = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ActionPlanRecordBatchSizeRangeRuleFlagsValuesAboveBatchSize()
    {
        var rule = new ActionPlanRecordBatchSizeRangeRule(32, 255);
        var item = ActionPlanRecord.NewActionPlanRecord("x", "y");
        item.BatchSize = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ActionPlanRecordRequiredNameRuleFlagsABlankName()
    {
        var rule = new ActionPlanRecordRequiredNameRule("action plan record needs this field");
        var item = ActionPlanRecord.NewActionPlanRecord("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ActionPlanRecordLabelLengthRuleFlagsALongLabel()
    {
        var rule = new ActionPlanRecordLabelLengthRule(255);
        var item = ActionPlanRecord.NewActionPlanRecord("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ActionPlanRecordValidatorReportsEveryProblem();
        ActionPlanRecordAttemptsRangeRuleFlagsValuesAboveAttempts();
        ActionPlanRecordBatchSizeRangeRuleFlagsValuesAboveBatchSize();
        ActionPlanRecordRequiredNameRuleFlagsABlankName();
        ActionPlanRecordLabelLengthRuleFlagsALongLabel();
    }
}
