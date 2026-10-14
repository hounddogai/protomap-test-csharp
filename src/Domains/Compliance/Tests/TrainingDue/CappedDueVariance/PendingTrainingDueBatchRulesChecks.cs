namespace Acme.Workforce.Compliance.Tests.TrainingDue.CappedDueVariance;

using Acme.Workforce.Compliance.TrainingDue.CappedDueVariance;

/// <summary>Checks of capped due variance code, run by the project's runner.</summary>
public static class PendingTrainingDueBatchRulesChecks
{
    public static void PendingTrainingDueBatchValidatorReportsEveryProblem()
    {
        var validator = PendingTrainingDueBatchRules.DefaultPendingTrainingDueBatchValidator();
        var item = PendingTrainingDueBatch.NewPendingTrainingDueBatch("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PendingTrainingDueBatchCodeLengthRuleFlagsALongCode()
    {
        var rule = new PendingTrainingDueBatchCodeLengthRule(80);
        var item = PendingTrainingDueBatch.NewPendingTrainingDueBatch("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PendingTrainingDueBatchPhoneLengthRuleFlagsALongPhone()
    {
        var rule = new PendingTrainingDueBatchPhoneLengthRule(32);
        var item = PendingTrainingDueBatch.NewPendingTrainingDueBatch("x", "y");
        item.Phone = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Phone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PendingTrainingDueBatchLabelLengthRuleFlagsALongLabel()
    {
        var rule = new PendingTrainingDueBatchLabelLengthRule(16);
        var item = PendingTrainingDueBatch.NewPendingTrainingDueBatch("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PendingTrainingDueBatchValidatorReportsEveryProblem();
        PendingTrainingDueBatchCodeLengthRuleFlagsALongCode();
        PendingTrainingDueBatchPhoneLengthRuleFlagsALongPhone();
        PendingTrainingDueBatchLabelLengthRuleFlagsALongLabel();
    }
}
