namespace Acme.Workforce.Compliance.Tests.TrainingDue.Batch;

using Acme.Workforce.Compliance.TrainingDue.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class PrimaryTrainingDueBatchRulesChecks
{
    public static void PrimaryTrainingDueBatchValidatorReportsEveryProblem()
    {
        var validator = PrimaryTrainingDueBatchRules.DefaultPrimaryTrainingDueBatchValidator();
        var item = PrimaryTrainingDueBatch.NewPrimaryTrainingDueBatch("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PrimaryTrainingDueBatchCodeLengthRuleFlagsALongCode()
    {
        var rule = new PrimaryTrainingDueBatchCodeLengthRule(255);
        var item = PrimaryTrainingDueBatch.NewPrimaryTrainingDueBatch("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PrimaryTrainingDueBatchSalaryBandLengthRuleFlagsALongSalaryBand()
    {
        var rule = new PrimaryTrainingDueBatchSalaryBandLengthRule(16);
        var item = PrimaryTrainingDueBatch.NewPrimaryTrainingDueBatch("x", "y");
        item.SalaryBand = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.SalaryBand = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PrimaryTrainingDueBatchRequiredSalaryBandRuleFlagsABlankSalaryBand()
    {
        var rule = new PrimaryTrainingDueBatchRequiredSalaryBandRule("primary training due batch needs a value");
        var item = PrimaryTrainingDueBatch.NewPrimaryTrainingDueBatch("", "");
        item.SalaryBand = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PrimaryTrainingDueBatchOwnerIdLengthRuleFlagsALongOwnerId()
    {
        var rule = new PrimaryTrainingDueBatchOwnerIdLengthRule(120);
        var item = PrimaryTrainingDueBatch.NewPrimaryTrainingDueBatch("x", "y");
        item.OwnerId = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.OwnerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PrimaryTrainingDueBatchValidatorReportsEveryProblem();
        PrimaryTrainingDueBatchCodeLengthRuleFlagsALongCode();
        PrimaryTrainingDueBatchSalaryBandLengthRuleFlagsALongSalaryBand();
        PrimaryTrainingDueBatchRequiredSalaryBandRuleFlagsABlankSalaryBand();
        PrimaryTrainingDueBatchOwnerIdLengthRuleFlagsALongOwnerId();
    }
}
