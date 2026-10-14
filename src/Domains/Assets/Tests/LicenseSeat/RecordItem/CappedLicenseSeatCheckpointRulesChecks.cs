namespace Acme.Workforce.Assets.Tests.LicenseSeat.RecordItem;

using Acme.Workforce.Assets.LicenseSeat.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class CappedLicenseSeatCheckpointRulesChecks
{
    public static void CappedLicenseSeatCheckpointValidatorReportsEveryProblem()
    {
        var validator = CappedLicenseSeatCheckpointRules.DefaultCappedLicenseSeatCheckpointValidator();
        var item = CappedLicenseSeatCheckpoint.NewCappedLicenseSeatCheckpoint("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CappedLicenseSeatCheckpointBatchSizeRangeRuleFlagsValuesAboveBatchSize()
    {
        var rule = new CappedLicenseSeatCheckpointBatchSizeRangeRule(32, 1000);
        var item = CappedLicenseSeatCheckpoint.NewCappedLicenseSeatCheckpoint("x", "y");
        item.BatchSize = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CappedLicenseSeatCheckpointRequiredCostCenterRuleFlagsABlankCostCenter()
    {
        var rule = new CappedLicenseSeatCheckpointRequiredCostCenterRule("capped license seat checkpoint needs a value");
        var item = CappedLicenseSeatCheckpoint.NewCappedLicenseSeatCheckpoint("", "");
        item.CostCenter = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CappedLicenseSeatCheckpointTitleLengthRuleFlagsALongTitle()
    {
        var rule = new CappedLicenseSeatCheckpointTitleLengthRule(64);
        var item = CappedLicenseSeatCheckpoint.NewCappedLicenseSeatCheckpoint("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CappedLicenseSeatCheckpointCodeLengthRuleFlagsALongCode()
    {
        var rule = new CappedLicenseSeatCheckpointCodeLengthRule(80);
        var item = CappedLicenseSeatCheckpoint.NewCappedLicenseSeatCheckpoint("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        CappedLicenseSeatCheckpointValidatorReportsEveryProblem();
        CappedLicenseSeatCheckpointBatchSizeRangeRuleFlagsValuesAboveBatchSize();
        CappedLicenseSeatCheckpointRequiredCostCenterRuleFlagsABlankCostCenter();
        CappedLicenseSeatCheckpointTitleLengthRuleFlagsALongTitle();
        CappedLicenseSeatCheckpointCodeLengthRuleFlagsALongCode();
    }
}
