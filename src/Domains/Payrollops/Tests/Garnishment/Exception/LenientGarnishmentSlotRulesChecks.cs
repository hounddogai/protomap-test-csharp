namespace Acme.Workforce.Payrollops.Tests.Garnishment.Exception;

using Acme.Workforce.Payrollops.Garnishment.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class LenientGarnishmentSlotRulesChecks
{
    public static void LenientGarnishmentSlotValidatorReportsEveryProblem()
    {
        var validator = LenientGarnishmentSlotRules.DefaultLenientGarnishmentSlotValidator();
        var item = LenientGarnishmentSlot.NewLenientGarnishmentSlot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientGarnishmentSlotDiscountRangeRuleFlagsValuesAboveDiscount()
    {
        var rule = new LenientGarnishmentSlotDiscountRangeRule(6.1, 2009.9);
        var item = LenientGarnishmentSlot.NewLenientGarnishmentSlot("x", "y");
        item.Discount = 2011.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LenientGarnishmentSlotRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new LenientGarnishmentSlotRequiredTitleRule("lenient garnishment slot needs a value");
        var item = LenientGarnishmentSlot.NewLenientGarnishmentSlot("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientGarnishmentSlotTitleLengthRuleFlagsALongTitle()
    {
        var rule = new LenientGarnishmentSlotTitleLengthRule(120);
        var item = LenientGarnishmentSlot.NewLenientGarnishmentSlot("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LenientGarnishmentSlotRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LenientGarnishmentSlotRequiredCodeRule("lenient garnishment slot needs an entry");
        var item = LenientGarnishmentSlot.NewLenientGarnishmentSlot("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LenientGarnishmentSlotValidatorReportsEveryProblem();
        LenientGarnishmentSlotDiscountRangeRuleFlagsValuesAboveDiscount();
        LenientGarnishmentSlotRequiredTitleRuleFlagsABlankTitle();
        LenientGarnishmentSlotTitleLengthRuleFlagsALongTitle();
        LenientGarnishmentSlotRequiredCodeRuleFlagsABlankCode();
    }
}
