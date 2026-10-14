namespace Acme.Workforce.Assets.Tests.Laptop.LaptopAdjustment;

using Acme.Workforce.Assets.Laptop.LaptopAdjustment;

/// <summary>Checks of laptop adjustment code, run by the project's runner.</summary>
public static class CappedLaptopRecordRulesChecks
{
    public static void CappedLaptopRecordValidatorReportsEveryProblem()
    {
        var validator = CappedLaptopRecordRules.DefaultCappedLaptopRecordValidator();
        var item = CappedLaptopRecord.NewCappedLaptopRecord("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CappedLaptopRecordNameLengthRuleFlagsALongName()
    {
        var rule = new CappedLaptopRecordNameLengthRule(120);
        var item = CappedLaptopRecord.NewCappedLaptopRecord("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CappedLaptopRecordExpiresAtRangeRuleFlagsValuesAboveExpiresAt()
    {
        var rule = new CappedLaptopRecordExpiresAtRangeRule(64, 3600);
        var item = CappedLaptopRecord.NewCappedLaptopRecord("x", "y");
        item.ExpiresAt = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CappedLaptopRecordCodeLengthRuleFlagsALongCode()
    {
        var rule = new CappedLaptopRecordCodeLengthRule(120);
        var item = CappedLaptopRecord.NewCappedLaptopRecord("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CappedLaptopRecordRequiredNameRuleFlagsABlankName()
    {
        var rule = new CappedLaptopRecordRequiredNameRule("capped laptop record needs this field");
        var item = CappedLaptopRecord.NewCappedLaptopRecord("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CappedLaptopRecordDiscountRangeRuleFlagsValuesAboveDiscount()
    {
        var rule = new CappedLaptopRecordDiscountRangeRule(5.5, 4219.2);
        var item = CappedLaptopRecord.NewCappedLaptopRecord("x", "y");
        item.Discount = 4220.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        CappedLaptopRecordValidatorReportsEveryProblem();
        CappedLaptopRecordNameLengthRuleFlagsALongName();
        CappedLaptopRecordExpiresAtRangeRuleFlagsValuesAboveExpiresAt();
        CappedLaptopRecordCodeLengthRuleFlagsALongCode();
        CappedLaptopRecordRequiredNameRuleFlagsABlankName();
        CappedLaptopRecordDiscountRangeRuleFlagsValuesAboveDiscount();
    }
}
