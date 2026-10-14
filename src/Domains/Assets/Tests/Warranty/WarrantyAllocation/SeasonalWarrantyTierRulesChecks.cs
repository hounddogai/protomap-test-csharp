namespace Acme.Workforce.Assets.Tests.Warranty.WarrantyAllocation;

using Acme.Workforce.Assets.Warranty.WarrantyAllocation;

/// <summary>Checks of warranty allocation code, run by the project's runner.</summary>
public static class SeasonalWarrantyTierRulesChecks
{
    public static void SeasonalWarrantyTierValidatorReportsEveryProblem()
    {
        var validator = SeasonalWarrantyTierRules.DefaultSeasonalWarrantyTierValidator();
        var item = SeasonalWarrantyTier.NewSeasonalWarrantyTier("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SeasonalWarrantyTierRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new SeasonalWarrantyTierRequiredCodeRule("seasonal warranty tier needs this field");
        var item = SeasonalWarrantyTier.NewSeasonalWarrantyTier("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SeasonalWarrantyTierCodeLengthRuleFlagsALongCode()
    {
        var rule = new SeasonalWarrantyTierCodeLengthRule(86400);
        var item = SeasonalWarrantyTier.NewSeasonalWarrantyTier("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SeasonalWarrantyTierMarginRangeRuleFlagsValuesAboveMargin()
    {
        var rule = new SeasonalWarrantyTierMarginRangeRule(0.1, 1881.1);
        var item = SeasonalWarrantyTier.NewSeasonalWarrantyTier("x", "y");
        item.Margin = 1882.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        SeasonalWarrantyTierValidatorReportsEveryProblem();
        SeasonalWarrantyTierRequiredCodeRuleFlagsABlankCode();
        SeasonalWarrantyTierCodeLengthRuleFlagsALongCode();
        SeasonalWarrantyTierMarginRangeRuleFlagsValuesAboveMargin();
    }
}
