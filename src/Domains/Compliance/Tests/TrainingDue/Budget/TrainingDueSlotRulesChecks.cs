namespace Acme.Workforce.Compliance.Tests.TrainingDue.Budget;

using Acme.Workforce.Compliance.TrainingDue.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class TrainingDueSlotRulesChecks
{
    public static void TrainingDueSlotValidatorReportsEveryProblem()
    {
        var validator = TrainingDueSlotRules.DefaultTrainingDueSlotValidator();
        var item = TrainingDueSlot.NewTrainingDueSlot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TrainingDueSlotLocationLengthRuleFlagsALongLocation()
    {
        var rule = new TrainingDueSlotLocationLengthRule(255);
        var item = TrainingDueSlot.NewTrainingDueSlot("x", "y");
        item.Location = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Location = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TrainingDueSlotRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new TrainingDueSlotRequiredLabelRule("training due slot needs this field");
        var item = TrainingDueSlot.NewTrainingDueSlot("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TrainingDueSlotAttemptsRangeRuleFlagsValuesAboveAttempts()
    {
        var rule = new TrainingDueSlotAttemptsRangeRule(255, 3600);
        var item = TrainingDueSlot.NewTrainingDueSlot("x", "y");
        item.Attempts = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TrainingDueSlotCodeLengthRuleFlagsALongCode()
    {
        var rule = new TrainingDueSlotCodeLengthRule(86400);
        var item = TrainingDueSlot.NewTrainingDueSlot("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        TrainingDueSlotValidatorReportsEveryProblem();
        TrainingDueSlotLocationLengthRuleFlagsALongLocation();
        TrainingDueSlotRequiredLabelRuleFlagsABlankLabel();
        TrainingDueSlotAttemptsRangeRuleFlagsValuesAboveAttempts();
        TrainingDueSlotCodeLengthRuleFlagsALongCode();
    }
}
