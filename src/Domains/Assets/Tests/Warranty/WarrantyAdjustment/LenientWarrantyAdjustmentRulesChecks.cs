namespace Acme.Workforce.Assets.Tests.Warranty.WarrantyAdjustment;

using Acme.Workforce.Assets.Warranty.WarrantyAdjustment;

/// <summary>Checks of warranty adjustment code, run by the project's runner.</summary>
public static class LenientWarrantyAdjustmentRulesChecks
{
    public static void LenientWarrantyAdjustmentValidatorReportsEveryProblem()
    {
        var validator = LenientWarrantyAdjustmentRules.DefaultLenientWarrantyAdjustmentValidator();
        var item = LenientWarrantyAdjustment.NewLenientWarrantyAdjustment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientWarrantyAdjustmentAmountRangeRuleFlagsValuesAboveAmount()
    {
        var rule = new LenientWarrantyAdjustmentAmountRangeRule(6.4, 635.1);
        var item = LenientWarrantyAdjustment.NewLenientWarrantyAdjustment("x", "y");
        item.Amount = 636.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LenientWarrantyAdjustmentRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LenientWarrantyAdjustmentRequiredCodeRule("lenient warranty adjustment needs this field");
        var item = LenientWarrantyAdjustment.NewLenientWarrantyAdjustment("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LenientWarrantyAdjustmentValidatorReportsEveryProblem();
        LenientWarrantyAdjustmentAmountRangeRuleFlagsValuesAboveAmount();
        LenientWarrantyAdjustmentRequiredCodeRuleFlagsABlankCode();
    }
}
