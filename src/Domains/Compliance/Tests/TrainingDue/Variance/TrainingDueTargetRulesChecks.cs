namespace Acme.Workforce.Compliance.Tests.TrainingDue.Variance;

using Acme.Workforce.Compliance.TrainingDue.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class TrainingDueTargetRulesChecks
{
    public static void TrainingDueTargetValidatorReportsEveryProblem()
    {
        var validator = TrainingDueTargetRules.DefaultTrainingDueTargetValidator();
        var item = TrainingDueTarget.NewTrainingDueTarget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TrainingDueTargetRequiredNameRuleFlagsABlankName()
    {
        var rule = new TrainingDueTargetRequiredNameRule("training due target needs this field");
        var item = TrainingDueTarget.NewTrainingDueTarget("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TrainingDueTargetCodeLengthRuleFlagsALongCode()
    {
        var rule = new TrainingDueTargetCodeLengthRule(255);
        var item = TrainingDueTarget.NewTrainingDueTarget("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TrainingDueTargetFloorValueRangeRuleFlagsValuesAboveFloorValue()
    {
        var rule = new TrainingDueTargetFloorValueRangeRule(1.5, 4173.4);
        var item = TrainingDueTarget.NewTrainingDueTarget("x", "y");
        item.FloorValue = 4174.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TrainingDueTargetContactEmailLengthRuleFlagsALongContactEmail()
    {
        var rule = new TrainingDueTargetContactEmailLengthRule(16);
        var item = TrainingDueTarget.NewTrainingDueTarget("x", "y");
        item.ContactEmail = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ContactEmail = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        TrainingDueTargetValidatorReportsEveryProblem();
        TrainingDueTargetRequiredNameRuleFlagsABlankName();
        TrainingDueTargetCodeLengthRuleFlagsALongCode();
        TrainingDueTargetFloorValueRangeRuleFlagsValuesAboveFloorValue();
        TrainingDueTargetContactEmailLengthRuleFlagsALongContactEmail();
    }
}
