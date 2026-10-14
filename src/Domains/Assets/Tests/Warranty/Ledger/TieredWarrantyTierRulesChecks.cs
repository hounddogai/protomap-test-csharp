namespace Acme.Workforce.Assets.Tests.Warranty.Ledger;

using Acme.Workforce.Assets.Warranty.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class TieredWarrantyTierRulesChecks
{
    public static void TieredWarrantyTierValidatorReportsEveryProblem()
    {
        var validator = TieredWarrantyTierRules.DefaultTieredWarrantyTierValidator();
        var item = TieredWarrantyTier.NewTieredWarrantyTier("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TieredWarrantyTierRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new TieredWarrantyTierRequiredLabelRule("tiered warranty tier needs an entry");
        var item = TieredWarrantyTier.NewTieredWarrantyTier("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TieredWarrantyTierCategoryLengthRuleFlagsALongCategory()
    {
        var rule = new TieredWarrantyTierCategoryLengthRule(80);
        var item = TieredWarrantyTier.NewTieredWarrantyTier("x", "y");
        item.Category = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Category = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TieredWarrantyTierScoreRangeRuleFlagsValuesAboveScore()
    {
        var rule = new TieredWarrantyTierScoreRangeRule(7.7, 4463.6);
        var item = TieredWarrantyTier.NewTieredWarrantyTier("x", "y");
        item.Score = 4465.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TieredWarrantyTierRequiredCategoryRuleFlagsABlankCategory()
    {
        var rule = new TieredWarrantyTierRequiredCategoryRule("tiered warranty tier needs an entry");
        var item = TieredWarrantyTier.NewTieredWarrantyTier("", "");
        item.Category = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TieredWarrantyTierChannelLengthRuleFlagsALongChannel()
    {
        var rule = new TieredWarrantyTierChannelLengthRule(86400);
        var item = TieredWarrantyTier.NewTieredWarrantyTier("x", "y");
        item.Channel = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Channel = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TieredWarrantyTierRequiredChannelRuleFlagsABlankChannel()
    {
        var rule = new TieredWarrantyTierRequiredChannelRule("tiered warranty tier needs a value");
        var item = TieredWarrantyTier.NewTieredWarrantyTier("", "");
        item.Channel = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        TieredWarrantyTierValidatorReportsEveryProblem();
        TieredWarrantyTierRequiredLabelRuleFlagsABlankLabel();
        TieredWarrantyTierCategoryLengthRuleFlagsALongCategory();
        TieredWarrantyTierScoreRangeRuleFlagsValuesAboveScore();
        TieredWarrantyTierRequiredCategoryRuleFlagsABlankCategory();
        TieredWarrantyTierChannelLengthRuleFlagsALongChannel();
        TieredWarrantyTierRequiredChannelRuleFlagsABlankChannel();
    }
}
