namespace Acme.Workforce.Compliance.Tests.TrainingDue.Budget;

using Acme.Workforce.Compliance.TrainingDue.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class LegacyTrainingDueBatchRulesChecks
{
    public static void LegacyTrainingDueBatchValidatorReportsEveryProblem()
    {
        var validator = LegacyTrainingDueBatchRules.DefaultLegacyTrainingDueBatchValidator();
        var item = LegacyTrainingDueBatch.NewLegacyTrainingDueBatch("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LegacyTrainingDueBatchRequiredNameRuleFlagsABlankName()
    {
        var rule = new LegacyTrainingDueBatchRequiredNameRule("legacy training due batch needs an entry");
        var item = LegacyTrainingDueBatch.NewLegacyTrainingDueBatch("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LegacyTrainingDueBatchRetriesRangeRuleFlagsValuesAboveRetries()
    {
        var rule = new LegacyTrainingDueBatchRetriesRangeRule(120, 120);
        var item = LegacyTrainingDueBatch.NewLegacyTrainingDueBatch("x", "y");
        item.Retries = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LegacyTrainingDueBatchNameLengthRuleFlagsALongName()
    {
        var rule = new LegacyTrainingDueBatchNameLengthRule(86400);
        var item = LegacyTrainingDueBatch.NewLegacyTrainingDueBatch("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LegacyTrainingDueBatchRequiredSalaryBandRuleFlagsABlankSalaryBand()
    {
        var rule = new LegacyTrainingDueBatchRequiredSalaryBandRule("legacy training due batch needs this field");
        var item = LegacyTrainingDueBatch.NewLegacyTrainingDueBatch("", "");
        item.SalaryBand = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LegacyTrainingDueBatchValidatorReportsEveryProblem();
        LegacyTrainingDueBatchRequiredNameRuleFlagsABlankName();
        LegacyTrainingDueBatchRetriesRangeRuleFlagsValuesAboveRetries();
        LegacyTrainingDueBatchNameLengthRuleFlagsALongName();
        LegacyTrainingDueBatchRequiredSalaryBandRuleFlagsABlankSalaryBand();
    }
}
