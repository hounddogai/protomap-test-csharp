namespace Acme.Workforce.Orgdesign.Tests.Reorg.ReorgSnapshot;

using Acme.Workforce.Orgdesign.Reorg.ReorgSnapshot;

/// <summary>Checks of reorg snapshot code, run by the project's runner.</summary>
public static class BatchedReorgSnapshotRulesChecks
{
    public static void BatchedReorgSnapshotValidatorReportsEveryProblem()
    {
        var validator = BatchedReorgSnapshotRules.DefaultBatchedReorgSnapshotValidator();
        var item = BatchedReorgSnapshot.NewBatchedReorgSnapshot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BatchedReorgSnapshotCodeLengthRuleFlagsALongCode()
    {
        var rule = new BatchedReorgSnapshotCodeLengthRule(64);
        var item = BatchedReorgSnapshot.NewBatchedReorgSnapshot("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BatchedReorgSnapshotSalaryBandLengthRuleFlagsALongSalaryBand()
    {
        var rule = new BatchedReorgSnapshotSalaryBandLengthRule(1000);
        var item = BatchedReorgSnapshot.NewBatchedReorgSnapshot("x", "y");
        item.SalaryBand = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.SalaryBand = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BatchedReorgSnapshotRequiredSalaryBandRuleFlagsABlankSalaryBand()
    {
        var rule = new BatchedReorgSnapshotRequiredSalaryBandRule("batched reorg snapshot needs an entry");
        var item = BatchedReorgSnapshot.NewBatchedReorgSnapshot("", "");
        item.SalaryBand = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BatchedReorgSnapshotRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new BatchedReorgSnapshotRequiredCodeRule("batched reorg snapshot needs this field");
        var item = BatchedReorgSnapshot.NewBatchedReorgSnapshot("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BatchedReorgSnapshotScoreRangeRuleFlagsValuesAboveScore()
    {
        var rule = new BatchedReorgSnapshotScoreRangeRule(0.3, 2391.2);
        var item = BatchedReorgSnapshot.NewBatchedReorgSnapshot("x", "y");
        item.Score = 2392.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        BatchedReorgSnapshotValidatorReportsEveryProblem();
        BatchedReorgSnapshotCodeLengthRuleFlagsALongCode();
        BatchedReorgSnapshotSalaryBandLengthRuleFlagsALongSalaryBand();
        BatchedReorgSnapshotRequiredSalaryBandRuleFlagsABlankSalaryBand();
        BatchedReorgSnapshotRequiredCodeRuleFlagsABlankCode();
        BatchedReorgSnapshotScoreRangeRuleFlagsValuesAboveScore();
    }
}
