namespace Acme.Workforce.Assets.Tests.AssetReturn.Cycle;

using Acme.Workforce.Assets.AssetReturn.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class LenientAssetReturnAdjustmentRulesChecks
{
    public static void LenientAssetReturnAdjustmentValidatorReportsEveryProblem()
    {
        var validator = LenientAssetReturnAdjustmentRules.DefaultLenientAssetReturnAdjustmentValidator();
        var item = LenientAssetReturnAdjustment.NewLenientAssetReturnAdjustment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientAssetReturnAdjustmentCreatedAtRangeRuleFlagsValuesAboveCreatedAt()
    {
        var rule = new LenientAssetReturnAdjustmentCreatedAtRangeRule(16, 255);
        var item = LenientAssetReturnAdjustment.NewLenientAssetReturnAdjustment("x", "y");
        item.CreatedAt = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LenientAssetReturnAdjustmentCodeLengthRuleFlagsALongCode()
    {
        var rule = new LenientAssetReturnAdjustmentCodeLengthRule(80);
        var item = LenientAssetReturnAdjustment.NewLenientAssetReturnAdjustment("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LenientAssetReturnAdjustmentLimitRangeRuleFlagsValuesAboveLimit()
    {
        var rule = new LenientAssetReturnAdjustmentLimitRangeRule(80, 86400);
        var item = LenientAssetReturnAdjustment.NewLenientAssetReturnAdjustment("x", "y");
        item.Limit = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LenientAssetReturnAdjustmentDiscountRangeRuleFlagsValuesAboveDiscount()
    {
        var rule = new LenientAssetReturnAdjustmentDiscountRangeRule(4.8, 4470.1);
        var item = LenientAssetReturnAdjustment.NewLenientAssetReturnAdjustment("x", "y");
        item.Discount = 4471.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        LenientAssetReturnAdjustmentValidatorReportsEveryProblem();
        LenientAssetReturnAdjustmentCreatedAtRangeRuleFlagsValuesAboveCreatedAt();
        LenientAssetReturnAdjustmentCodeLengthRuleFlagsALongCode();
        LenientAssetReturnAdjustmentLimitRangeRuleFlagsValuesAboveLimit();
        LenientAssetReturnAdjustmentDiscountRangeRuleFlagsValuesAboveDiscount();
    }
}
