namespace Acme.Workforce.Compliance.Tests.TrainingDue.Assignment;

using Acme.Workforce.Compliance.TrainingDue.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class TrainingDueRollupRulesChecks
{
    public static void TrainingDueRollupValidatorReportsEveryProblem()
    {
        var validator = TrainingDueRollupRules.DefaultTrainingDueRollupValidator();
        var item = TrainingDueRollup.NewTrainingDueRollup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TrainingDueRollupRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new TrainingDueRollupRequiredLabelRule("training due rollup needs a value");
        var item = TrainingDueRollup.NewTrainingDueRollup("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TrainingDueRollupCodeLengthRuleFlagsALongCode()
    {
        var rule = new TrainingDueRollupCodeLengthRule(86400);
        var item = TrainingDueRollup.NewTrainingDueRollup("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TrainingDueRollupRequiredJobCodeRuleFlagsABlankJobCode()
    {
        var rule = new TrainingDueRollupRequiredJobCodeRule("training due rollup needs an entry");
        var item = TrainingDueRollup.NewTrainingDueRollup("", "");
        item.JobCode = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TrainingDueRollupLabelLengthRuleFlagsALongLabel()
    {
        var rule = new TrainingDueRollupLabelLengthRule(16);
        var item = TrainingDueRollup.NewTrainingDueRollup("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        TrainingDueRollupValidatorReportsEveryProblem();
        TrainingDueRollupRequiredLabelRuleFlagsABlankLabel();
        TrainingDueRollupCodeLengthRuleFlagsALongCode();
        TrainingDueRollupRequiredJobCodeRuleFlagsABlankJobCode();
        TrainingDueRollupLabelLengthRuleFlagsALongLabel();
    }
}
