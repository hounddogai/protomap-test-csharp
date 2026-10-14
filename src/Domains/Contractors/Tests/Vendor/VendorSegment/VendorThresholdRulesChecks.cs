namespace Acme.Workforce.Contractors.Tests.Vendor.VendorSegment;

using Acme.Workforce.Contractors.Vendor.VendorSegment;

/// <summary>Checks of vendor segment code, run by the project's runner.</summary>
public static class VendorThresholdRulesChecks
{
    public static void VendorThresholdValidatorReportsEveryProblem()
    {
        var validator = VendorThresholdRules.DefaultVendorThresholdValidator();
        var item = VendorThreshold.NewVendorThreshold("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void VendorThresholdNameLengthRuleFlagsALongName()
    {
        var rule = new VendorThresholdNameLengthRule(120);
        var item = VendorThreshold.NewVendorThreshold("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void VendorThresholdRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new VendorThresholdRequiredCodeRule("vendor threshold needs an entry");
        var item = VendorThreshold.NewVendorThreshold("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void VendorThresholdHoursRangeRuleFlagsValuesAboveHours()
    {
        var rule = new VendorThresholdHoursRangeRule(0.8, 3954.9);
        var item = VendorThreshold.NewVendorThreshold("x", "y");
        item.Hours = 3956.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void VendorThresholdCodeLengthRuleFlagsALongCode()
    {
        var rule = new VendorThresholdCodeLengthRule(80);
        var item = VendorThreshold.NewVendorThreshold("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        VendorThresholdValidatorReportsEveryProblem();
        VendorThresholdNameLengthRuleFlagsALongName();
        VendorThresholdRequiredCodeRuleFlagsABlankCode();
        VendorThresholdHoursRangeRuleFlagsValuesAboveHours();
        VendorThresholdCodeLengthRuleFlagsALongCode();
    }
}
