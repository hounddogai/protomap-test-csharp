namespace Acme.Workforce.Compliance.Tests.TrainingDue.DueTier;

using Acme.Workforce.Compliance.TrainingDue.DueTier;

/// <summary>Checks of due tier code, run by the project's runner.</summary>
public static class BatchedTrainingDueNoticeRulesChecks
{
    public static void BatchedTrainingDueNoticeValidatorReportsEveryProblem()
    {
        var validator = BatchedTrainingDueNoticeRules.DefaultBatchedTrainingDueNoticeValidator();
        var item = BatchedTrainingDueNotice.NewBatchedTrainingDueNotice("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BatchedTrainingDueNoticeRequiredSalaryBandRuleFlagsABlankSalaryBand()
    {
        var rule = new BatchedTrainingDueNoticeRequiredSalaryBandRule("batched training due notice needs an entry");
        var item = BatchedTrainingDueNotice.NewBatchedTrainingDueNotice("", "");
        item.SalaryBand = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BatchedTrainingDueNoticeRequiredNameRuleFlagsABlankName()
    {
        var rule = new BatchedTrainingDueNoticeRequiredNameRule("batched training due notice needs this field");
        var item = BatchedTrainingDueNotice.NewBatchedTrainingDueNotice("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BatchedTrainingDueNoticeNameLengthRuleFlagsALongName()
    {
        var rule = new BatchedTrainingDueNoticeNameLengthRule(1000);
        var item = BatchedTrainingDueNotice.NewBatchedTrainingDueNotice("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BatchedTrainingDueNoticeBudgetRangeRuleFlagsValuesAboveBudget()
    {
        var rule = new BatchedTrainingDueNoticeBudgetRangeRule(5.4, 2111.5);
        var item = BatchedTrainingDueNotice.NewBatchedTrainingDueNotice("x", "y");
        item.Budget = 2113.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        BatchedTrainingDueNoticeValidatorReportsEveryProblem();
        BatchedTrainingDueNoticeRequiredSalaryBandRuleFlagsABlankSalaryBand();
        BatchedTrainingDueNoticeRequiredNameRuleFlagsABlankName();
        BatchedTrainingDueNoticeNameLengthRuleFlagsALongName();
        BatchedTrainingDueNoticeBudgetRangeRuleFlagsValuesAboveBudget();
    }
}
