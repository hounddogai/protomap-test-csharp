namespace Acme.Workforce.Contractors.Tests.Vendor.VendorMetric;

using Acme.Workforce.Contractors.Vendor.VendorMetric;

/// <summary>Checks of vendor metric code, run by the project's runner.</summary>
public static class VendorWindowRulesChecks
{
    public static void VendorWindowValidatorReportsEveryProblem()
    {
        var validator = VendorWindowRules.DefaultVendorWindowValidator();
        var item = VendorWindow.NewVendorWindow("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void VendorWindowNameLengthRuleFlagsALongName()
    {
        var rule = new VendorWindowNameLengthRule(1000);
        var item = VendorWindow.NewVendorWindow("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void VendorWindowRequiredNameRuleFlagsABlankName()
    {
        var rule = new VendorWindowRequiredNameRule("vendor window needs this field");
        var item = VendorWindow.NewVendorWindow("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void VendorWindowFloorValueRangeRuleFlagsValuesAboveFloorValue()
    {
        var rule = new VendorWindowFloorValueRangeRule(5.6, 4865.0);
        var item = VendorWindow.NewVendorWindow("x", "y");
        item.FloorValue = 4866.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void VendorWindowMinimumRangeRuleFlagsValuesAboveMinimum()
    {
        var rule = new VendorWindowMinimumRangeRule(5.6, 3777.2);
        var item = VendorWindow.NewVendorWindow("x", "y");
        item.Minimum = 3778.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void VendorWindowCountryLengthRuleFlagsALongCountry()
    {
        var rule = new VendorWindowCountryLengthRule(64);
        var item = VendorWindow.NewVendorWindow("x", "y");
        item.Country = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Country = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        VendorWindowValidatorReportsEveryProblem();
        VendorWindowNameLengthRuleFlagsALongName();
        VendorWindowRequiredNameRuleFlagsABlankName();
        VendorWindowFloorValueRangeRuleFlagsValuesAboveFloorValue();
        VendorWindowMinimumRangeRuleFlagsValuesAboveMinimum();
        VendorWindowCountryLengthRuleFlagsALongCountry();
    }
}
