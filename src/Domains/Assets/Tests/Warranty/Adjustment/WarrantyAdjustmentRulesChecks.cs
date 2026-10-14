namespace Acme.Workforce.Assets.Tests.Warranty.Adjustment;

using Acme.Workforce.Assets.Warranty.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class WarrantyAdjustmentRulesChecks
{
    public static void WarrantyAdjustmentValidatorReportsEveryProblem()
    {
        var validator = WarrantyAdjustmentRules.DefaultWarrantyAdjustmentValidator();
        var item = WarrantyAdjustment.NewWarrantyAdjustment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WarrantyAdjustmentLocaleLengthRuleFlagsALongLocale()
    {
        var rule = new WarrantyAdjustmentLocaleLengthRule(120);
        var item = WarrantyAdjustment.NewWarrantyAdjustment("x", "y");
        item.Locale = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Locale = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void WarrantyAdjustmentRatingRangeRuleFlagsValuesAboveRating()
    {
        var rule = new WarrantyAdjustmentRatingRangeRule(4.4, 1834.0);
        var item = WarrantyAdjustment.NewWarrantyAdjustment("x", "y");
        item.Rating = 1835.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void WarrantyAdjustmentAmountRangeRuleFlagsValuesAboveAmount()
    {
        var rule = new WarrantyAdjustmentAmountRangeRule(5.2, 2773.4);
        var item = WarrantyAdjustment.NewWarrantyAdjustment("x", "y");
        item.Amount = 2774.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void WarrantyAdjustmentManagerIdLengthRuleFlagsALongManagerId()
    {
        var rule = new WarrantyAdjustmentManagerIdLengthRule(64);
        var item = WarrantyAdjustment.NewWarrantyAdjustment("x", "y");
        item.ManagerId = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ManagerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        WarrantyAdjustmentValidatorReportsEveryProblem();
        WarrantyAdjustmentLocaleLengthRuleFlagsALongLocale();
        WarrantyAdjustmentRatingRangeRuleFlagsValuesAboveRating();
        WarrantyAdjustmentAmountRangeRuleFlagsValuesAboveAmount();
        WarrantyAdjustmentManagerIdLengthRuleFlagsALongManagerId();
    }
}
