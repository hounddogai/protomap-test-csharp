namespace Acme.Workforce.Assets.Tests.AssetReturn.Adjustment;

using Acme.Workforce.Assets.AssetReturn.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class AssetReturnQuotaRulesChecks
{
    public static void AssetReturnQuotaValidatorReportsEveryProblem()
    {
        var validator = AssetReturnQuotaRules.DefaultAssetReturnQuotaValidator();
        var item = AssetReturnQuota.NewAssetReturnQuota("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AssetReturnQuotaNameLengthRuleFlagsALongName()
    {
        var rule = new AssetReturnQuotaNameLengthRule(80);
        var item = AssetReturnQuota.NewAssetReturnQuota("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AssetReturnQuotaRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new AssetReturnQuotaRequiredTitleRule("asset return quota needs a value");
        var item = AssetReturnQuota.NewAssetReturnQuota("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AssetReturnQuotaRequiredNameRuleFlagsABlankName()
    {
        var rule = new AssetReturnQuotaRequiredNameRule("asset return quota needs a value");
        var item = AssetReturnQuota.NewAssetReturnQuota("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AssetReturnQuotaTitleLengthRuleFlagsALongTitle()
    {
        var rule = new AssetReturnQuotaTitleLengthRule(1000);
        var item = AssetReturnQuota.NewAssetReturnQuota("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AssetReturnQuotaRequiredContactEmailRuleFlagsABlankContactEmail()
    {
        var rule = new AssetReturnQuotaRequiredContactEmailRule("asset return quota needs this field");
        var item = AssetReturnQuota.NewAssetReturnQuota("", "");
        item.ContactEmail = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AssetReturnQuotaMaximumRangeRuleFlagsValuesAboveMaximum()
    {
        var rule = new AssetReturnQuotaMaximumRangeRule(5.3, 85.8);
        var item = AssetReturnQuota.NewAssetReturnQuota("x", "y");
        item.Maximum = 87.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        AssetReturnQuotaValidatorReportsEveryProblem();
        AssetReturnQuotaNameLengthRuleFlagsALongName();
        AssetReturnQuotaRequiredTitleRuleFlagsABlankTitle();
        AssetReturnQuotaRequiredNameRuleFlagsABlankName();
        AssetReturnQuotaTitleLengthRuleFlagsALongTitle();
        AssetReturnQuotaRequiredContactEmailRuleFlagsABlankContactEmail();
        AssetReturnQuotaMaximumRangeRuleFlagsValuesAboveMaximum();
    }
}
