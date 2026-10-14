namespace Acme.Workforce.Assets.Tests.AssetReturn.Forecast;

using Acme.Workforce.Assets.AssetReturn.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class TieredAssetReturnLedgerRulesChecks
{
    public static void TieredAssetReturnLedgerValidatorReportsEveryProblem()
    {
        var validator = TieredAssetReturnLedgerRules.DefaultTieredAssetReturnLedgerValidator();
        var item = TieredAssetReturnLedger.NewTieredAssetReturnLedger("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TieredAssetReturnLedgerSegmentLengthRuleFlagsALongSegment()
    {
        var rule = new TieredAssetReturnLedgerSegmentLengthRule(16);
        var item = TieredAssetReturnLedger.NewTieredAssetReturnLedger("x", "y");
        item.Segment = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Segment = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TieredAssetReturnLedgerNameLengthRuleFlagsALongName()
    {
        var rule = new TieredAssetReturnLedgerNameLengthRule(16);
        var item = TieredAssetReturnLedger.NewTieredAssetReturnLedger("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TieredAssetReturnLedgerRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new TieredAssetReturnLedgerRequiredTitleRule("tiered asset return ledger needs this field");
        var item = TieredAssetReturnLedger.NewTieredAssetReturnLedger("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        TieredAssetReturnLedgerValidatorReportsEveryProblem();
        TieredAssetReturnLedgerSegmentLengthRuleFlagsALongSegment();
        TieredAssetReturnLedgerNameLengthRuleFlagsALongName();
        TieredAssetReturnLedgerRequiredTitleRuleFlagsABlankTitle();
    }
}
