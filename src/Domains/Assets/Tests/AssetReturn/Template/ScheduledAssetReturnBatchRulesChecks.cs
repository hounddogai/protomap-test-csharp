namespace Acme.Workforce.Assets.Tests.AssetReturn.Template;

using Acme.Workforce.Assets.AssetReturn.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class ScheduledAssetReturnBatchRulesChecks
{
    public static void ScheduledAssetReturnBatchValidatorReportsEveryProblem()
    {
        var validator = ScheduledAssetReturnBatchRules.DefaultScheduledAssetReturnBatchValidator();
        var item = ScheduledAssetReturnBatch.NewScheduledAssetReturnBatch("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScheduledAssetReturnBatchBudgetRangeRuleFlagsValuesAboveBudget()
    {
        var rule = new ScheduledAssetReturnBatchBudgetRangeRule(1.5, 4759.1);
        var item = ScheduledAssetReturnBatch.NewScheduledAssetReturnBatch("x", "y");
        item.Budget = 4760.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ScheduledAssetReturnBatchNotesLengthRuleFlagsALongNotes()
    {
        var rule = new ScheduledAssetReturnBatchNotesLengthRule(120);
        var item = ScheduledAssetReturnBatch.NewScheduledAssetReturnBatch("x", "y");
        item.Notes = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Notes = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScheduledAssetReturnBatchNameLengthRuleFlagsALongName()
    {
        var rule = new ScheduledAssetReturnBatchNameLengthRule(255);
        var item = ScheduledAssetReturnBatch.NewScheduledAssetReturnBatch("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScheduledAssetReturnBatchGradeLengthRuleFlagsALongGrade()
    {
        var rule = new ScheduledAssetReturnBatchGradeLengthRule(16);
        var item = ScheduledAssetReturnBatch.NewScheduledAssetReturnBatch("x", "y");
        item.Grade = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Grade = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScheduledAssetReturnBatchRequiredNameRuleFlagsABlankName()
    {
        var rule = new ScheduledAssetReturnBatchRequiredNameRule("scheduled asset return batch needs a value");
        var item = ScheduledAssetReturnBatch.NewScheduledAssetReturnBatch("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ScheduledAssetReturnBatchValidatorReportsEveryProblem();
        ScheduledAssetReturnBatchBudgetRangeRuleFlagsValuesAboveBudget();
        ScheduledAssetReturnBatchNotesLengthRuleFlagsALongNotes();
        ScheduledAssetReturnBatchNameLengthRuleFlagsALongName();
        ScheduledAssetReturnBatchGradeLengthRuleFlagsALongGrade();
        ScheduledAssetReturnBatchRequiredNameRuleFlagsABlankName();
    }
}
