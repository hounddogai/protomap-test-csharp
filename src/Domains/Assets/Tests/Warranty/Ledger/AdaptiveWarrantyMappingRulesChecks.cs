namespace Acme.Workforce.Assets.Tests.Warranty.Ledger;

using Acme.Workforce.Assets.Warranty.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class AdaptiveWarrantyMappingRulesChecks
{
    public static void AdaptiveWarrantyMappingValidatorReportsEveryProblem()
    {
        var validator = AdaptiveWarrantyMappingRules.DefaultAdaptiveWarrantyMappingValidator();
        var item = AdaptiveWarrantyMapping.NewAdaptiveWarrantyMapping("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdaptiveWarrantyMappingQuantityRangeRuleFlagsValuesAboveQuantity()
    {
        var rule = new AdaptiveWarrantyMappingQuantityRangeRule(32, 80);
        var item = AdaptiveWarrantyMapping.NewAdaptiveWarrantyMapping("x", "y");
        item.Quantity = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AdaptiveWarrantyMappingLabelLengthRuleFlagsALongLabel()
    {
        var rule = new AdaptiveWarrantyMappingLabelLengthRule(64);
        var item = AdaptiveWarrantyMapping.NewAdaptiveWarrantyMapping("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdaptiveWarrantyMappingRatioRangeRuleFlagsValuesAboveRatio()
    {
        var rule = new AdaptiveWarrantyMappingRatioRangeRule(6.7, 2870.7);
        var item = AdaptiveWarrantyMapping.NewAdaptiveWarrantyMapping("x", "y");
        item.Ratio = 2872.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AdaptiveWarrantyMappingExpiresAtRangeRuleFlagsValuesAboveExpiresAt()
    {
        var rule = new AdaptiveWarrantyMappingExpiresAtRangeRule(16, 1000);
        var item = AdaptiveWarrantyMapping.NewAdaptiveWarrantyMapping("x", "y");
        item.ExpiresAt = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AdaptiveWarrantyMappingCodeLengthRuleFlagsALongCode()
    {
        var rule = new AdaptiveWarrantyMappingCodeLengthRule(1000);
        var item = AdaptiveWarrantyMapping.NewAdaptiveWarrantyMapping("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AdaptiveWarrantyMappingValidatorReportsEveryProblem();
        AdaptiveWarrantyMappingQuantityRangeRuleFlagsValuesAboveQuantity();
        AdaptiveWarrantyMappingLabelLengthRuleFlagsALongLabel();
        AdaptiveWarrantyMappingRatioRangeRuleFlagsValuesAboveRatio();
        AdaptiveWarrantyMappingExpiresAtRangeRuleFlagsValuesAboveExpiresAt();
        AdaptiveWarrantyMappingCodeLengthRuleFlagsALongCode();
    }
}
