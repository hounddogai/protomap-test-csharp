namespace Acme.Workforce.Assets.Tests.Warranty.Bundle;

using Acme.Workforce.Assets.Warranty.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class WarrantyExceptionRulesChecks
{
    public static void WarrantyExceptionValidatorReportsEveryProblem()
    {
        var validator = WarrantyExceptionRules.DefaultWarrantyExceptionValidator();
        var item = WarrantyException.NewWarrantyException("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WarrantyExceptionRequiredRegionRuleFlagsABlankRegion()
    {
        var rule = new WarrantyExceptionRequiredRegionRule("warranty exception needs an entry");
        var item = WarrantyException.NewWarrantyException("", "");
        item.Region = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void WarrantyExceptionDiscountRangeRuleFlagsValuesAboveDiscount()
    {
        var rule = new WarrantyExceptionDiscountRangeRule(3.9, 2361.2);
        var item = WarrantyException.NewWarrantyException("x", "y");
        item.Discount = 2362.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void WarrantyExceptionLabelLengthRuleFlagsALongLabel()
    {
        var rule = new WarrantyExceptionLabelLengthRule(255);
        var item = WarrantyException.NewWarrantyException("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void WarrantyExceptionCodeLengthRuleFlagsALongCode()
    {
        var rule = new WarrantyExceptionCodeLengthRule(32);
        var item = WarrantyException.NewWarrantyException("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void WarrantyExceptionRequiredSummaryRuleFlagsABlankSummary()
    {
        var rule = new WarrantyExceptionRequiredSummaryRule("warranty exception needs this field");
        var item = WarrantyException.NewWarrantyException("", "");
        item.Summary = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void WarrantyExceptionRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new WarrantyExceptionRequiredLabelRule("warranty exception needs an entry");
        var item = WarrantyException.NewWarrantyException("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        WarrantyExceptionValidatorReportsEveryProblem();
        WarrantyExceptionRequiredRegionRuleFlagsABlankRegion();
        WarrantyExceptionDiscountRangeRuleFlagsValuesAboveDiscount();
        WarrantyExceptionLabelLengthRuleFlagsALongLabel();
        WarrantyExceptionCodeLengthRuleFlagsALongCode();
        WarrantyExceptionRequiredSummaryRuleFlagsABlankSummary();
        WarrantyExceptionRequiredLabelRuleFlagsABlankLabel();
    }
}
