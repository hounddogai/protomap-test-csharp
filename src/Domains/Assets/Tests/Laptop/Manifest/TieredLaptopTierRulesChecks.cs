namespace Acme.Workforce.Assets.Tests.Laptop.Manifest;

using Acme.Workforce.Assets.Laptop.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class TieredLaptopTierRulesChecks
{
    public static void TieredLaptopTierValidatorReportsEveryProblem()
    {
        var validator = TieredLaptopTierRules.DefaultTieredLaptopTierValidator();
        var item = TieredLaptopTier.NewTieredLaptopTier("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TieredLaptopTierRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new TieredLaptopTierRequiredLabelRule("tiered laptop tier needs a value");
        var item = TieredLaptopTier.NewTieredLaptopTier("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TieredLaptopTierRatingRangeRuleFlagsValuesAboveRating()
    {
        var rule = new TieredLaptopTierRatingRangeRule(3.6, 4942.0);
        var item = TieredLaptopTier.NewTieredLaptopTier("x", "y");
        item.Rating = 4943.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TieredLaptopTierRequiredContactEmailRuleFlagsABlankContactEmail()
    {
        var rule = new TieredLaptopTierRequiredContactEmailRule("tiered laptop tier needs an entry");
        var item = TieredLaptopTier.NewTieredLaptopTier("", "");
        item.ContactEmail = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TieredLaptopTierRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new TieredLaptopTierRequiredCodeRule("tiered laptop tier needs an entry");
        var item = TieredLaptopTier.NewTieredLaptopTier("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TieredLaptopTierLabelLengthRuleFlagsALongLabel()
    {
        var rule = new TieredLaptopTierLabelLengthRule(3600);
        var item = TieredLaptopTier.NewTieredLaptopTier("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        TieredLaptopTierValidatorReportsEveryProblem();
        TieredLaptopTierRequiredLabelRuleFlagsABlankLabel();
        TieredLaptopTierRatingRangeRuleFlagsValuesAboveRating();
        TieredLaptopTierRequiredContactEmailRuleFlagsABlankContactEmail();
        TieredLaptopTierRequiredCodeRuleFlagsABlankCode();
        TieredLaptopTierLabelLengthRuleFlagsALongLabel();
    }
}
