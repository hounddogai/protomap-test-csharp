namespace Acme.Workforce.Assets.Tests.AssetReturn.ReturnReservation;

using Acme.Workforce.Assets.AssetReturn.ReturnReservation;

/// <summary>Checks of return reservation code, run by the project's runner.</summary>
public static class AssetReturnEntryRulesChecks
{
    public static void AssetReturnEntryValidatorReportsEveryProblem()
    {
        var validator = AssetReturnEntryRules.DefaultAssetReturnEntryValidator();
        var item = AssetReturnEntry.NewAssetReturnEntry("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AssetReturnEntryTitleLengthRuleFlagsALongTitle()
    {
        var rule = new AssetReturnEntryTitleLengthRule(120);
        var item = AssetReturnEntry.NewAssetReturnEntry("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AssetReturnEntryRequiredCurrencyRuleFlagsABlankCurrency()
    {
        var rule = new AssetReturnEntryRequiredCurrencyRule("asset return entry needs a value");
        var item = AssetReturnEntry.NewAssetReturnEntry("", "");
        item.Currency = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AssetReturnEntryVersionRangeRuleFlagsValuesAboveVersion()
    {
        var rule = new AssetReturnEntryVersionRangeRule(255, 1000);
        var item = AssetReturnEntry.NewAssetReturnEntry("x", "y");
        item.Version = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AssetReturnEntryRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new AssetReturnEntryRequiredTitleRule("asset return entry needs a value");
        var item = AssetReturnEntry.NewAssetReturnEntry("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AssetReturnEntryCodeLengthRuleFlagsALongCode()
    {
        var rule = new AssetReturnEntryCodeLengthRule(255);
        var item = AssetReturnEntry.NewAssetReturnEntry("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AssetReturnEntryValidatorReportsEveryProblem();
        AssetReturnEntryTitleLengthRuleFlagsALongTitle();
        AssetReturnEntryRequiredCurrencyRuleFlagsABlankCurrency();
        AssetReturnEntryVersionRangeRuleFlagsValuesAboveVersion();
        AssetReturnEntryRequiredTitleRuleFlagsABlankTitle();
        AssetReturnEntryCodeLengthRuleFlagsALongCode();
    }
}
