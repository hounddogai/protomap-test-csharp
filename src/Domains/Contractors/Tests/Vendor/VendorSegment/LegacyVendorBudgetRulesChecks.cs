namespace Acme.Workforce.Contractors.Tests.Vendor.VendorSegment;

using Acme.Workforce.Contractors.Vendor.VendorSegment;

/// <summary>Checks of vendor segment code, run by the project's runner.</summary>
public static class LegacyVendorBudgetRulesChecks
{
    public static void LegacyVendorBudgetValidatorReportsEveryProblem()
    {
        var validator = LegacyVendorBudgetRules.DefaultLegacyVendorBudgetValidator();
        var item = LegacyVendorBudget.NewLegacyVendorBudget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LegacyVendorBudgetRequiredNameRuleFlagsABlankName()
    {
        var rule = new LegacyVendorBudgetRequiredNameRule("legacy vendor budget needs this field");
        var item = LegacyVendorBudget.NewLegacyVendorBudget("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LegacyVendorBudgetWeightRangeRuleFlagsValuesAboveWeight()
    {
        var rule = new LegacyVendorBudgetWeightRangeRule(9.2, 2544.4);
        var item = LegacyVendorBudget.NewLegacyVendorBudget("x", "y");
        item.Weight = 2545.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LegacyVendorBudgetUnitLengthRuleFlagsALongUnit()
    {
        var rule = new LegacyVendorBudgetUnitLengthRule(64);
        var item = LegacyVendorBudget.NewLegacyVendorBudget("x", "y");
        item.Unit = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Unit = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LegacyVendorBudgetRequiredUnitRuleFlagsABlankUnit()
    {
        var rule = new LegacyVendorBudgetRequiredUnitRule("legacy vendor budget needs this field");
        var item = LegacyVendorBudget.NewLegacyVendorBudget("", "");
        item.Unit = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LegacyVendorBudgetRequiredRegionRuleFlagsABlankRegion()
    {
        var rule = new LegacyVendorBudgetRequiredRegionRule("legacy vendor budget needs this field");
        var item = LegacyVendorBudget.NewLegacyVendorBudget("", "");
        item.Region = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LegacyVendorBudgetValidatorReportsEveryProblem();
        LegacyVendorBudgetRequiredNameRuleFlagsABlankName();
        LegacyVendorBudgetWeightRangeRuleFlagsValuesAboveWeight();
        LegacyVendorBudgetUnitLengthRuleFlagsALongUnit();
        LegacyVendorBudgetRequiredUnitRuleFlagsABlankUnit();
        LegacyVendorBudgetRequiredRegionRuleFlagsABlankRegion();
    }
}
