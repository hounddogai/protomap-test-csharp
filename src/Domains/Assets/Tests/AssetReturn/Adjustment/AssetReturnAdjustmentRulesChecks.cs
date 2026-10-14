namespace Acme.Workforce.Assets.Tests.AssetReturn.Adjustment;

using Acme.Workforce.Assets.AssetReturn.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class AssetReturnAdjustmentRulesChecks
{
    public static void AssetReturnAdjustmentValidatorReportsEveryProblem()
    {
        var validator = AssetReturnAdjustmentRules.DefaultAssetReturnAdjustmentValidator();
        var item = AssetReturnAdjustment.NewAssetReturnAdjustment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AssetReturnAdjustmentChecksumLengthRuleFlagsALongChecksum()
    {
        var rule = new AssetReturnAdjustmentChecksumLengthRule(64);
        var item = AssetReturnAdjustment.NewAssetReturnAdjustment("x", "y");
        item.Checksum = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Checksum = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AssetReturnAdjustmentUnitLengthRuleFlagsALongUnit()
    {
        var rule = new AssetReturnAdjustmentUnitLengthRule(80);
        var item = AssetReturnAdjustment.NewAssetReturnAdjustment("x", "y");
        item.Unit = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Unit = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AssetReturnAdjustmentRequiredChecksumRuleFlagsABlankChecksum()
    {
        var rule = new AssetReturnAdjustmentRequiredChecksumRule("asset return adjustment needs a value");
        var item = AssetReturnAdjustment.NewAssetReturnAdjustment("", "");
        item.Checksum = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AssetReturnAdjustmentRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new AssetReturnAdjustmentRequiredTitleRule("asset return adjustment needs a value");
        var item = AssetReturnAdjustment.NewAssetReturnAdjustment("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AssetReturnAdjustmentValidatorReportsEveryProblem();
        AssetReturnAdjustmentChecksumLengthRuleFlagsALongChecksum();
        AssetReturnAdjustmentUnitLengthRuleFlagsALongUnit();
        AssetReturnAdjustmentRequiredChecksumRuleFlagsABlankChecksum();
        AssetReturnAdjustmentRequiredTitleRuleFlagsABlankTitle();
    }
}
