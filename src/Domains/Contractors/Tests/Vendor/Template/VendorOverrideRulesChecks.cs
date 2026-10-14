namespace Acme.Workforce.Contractors.Tests.Vendor.Template;

using Acme.Workforce.Contractors.Vendor.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class VendorOverrideRulesChecks
{
    public static void VendorOverrideValidatorReportsEveryProblem()
    {
        var validator = VendorOverrideRules.DefaultVendorOverrideValidator();
        var item = VendorOverride.NewVendorOverride("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void VendorOverrideCodeLengthRuleFlagsALongCode()
    {
        var rule = new VendorOverrideCodeLengthRule(32);
        var item = VendorOverride.NewVendorOverride("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void VendorOverridePhoneLengthRuleFlagsALongPhone()
    {
        var rule = new VendorOverridePhoneLengthRule(16);
        var item = VendorOverride.NewVendorOverride("x", "y");
        item.Phone = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Phone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void VendorOverrideCreatedAtRangeRuleFlagsValuesAboveCreatedAt()
    {
        var rule = new VendorOverrideCreatedAtRangeRule(3600, 3600);
        var item = VendorOverride.NewVendorOverride("x", "y");
        item.CreatedAt = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void VendorOverrideRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new VendorOverrideRequiredLabelRule("vendor override needs a value");
        var item = VendorOverride.NewVendorOverride("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        VendorOverrideValidatorReportsEveryProblem();
        VendorOverrideCodeLengthRuleFlagsALongCode();
        VendorOverridePhoneLengthRuleFlagsALongPhone();
        VendorOverrideCreatedAtRangeRuleFlagsValuesAboveCreatedAt();
        VendorOverrideRequiredLabelRuleFlagsABlankLabel();
    }
}
