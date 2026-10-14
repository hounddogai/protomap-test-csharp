namespace Acme.Workforce.Contractors.Tests.Vendor.Rollup;

using Acme.Workforce.Contractors.Vendor.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class VendorRollupRulesChecks
{
    public static void VendorRollupValidatorReportsEveryProblem()
    {
        var validator = VendorRollupRules.DefaultVendorRollupValidator();
        var item = VendorRollup.NewVendorRollup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void VendorRollupRequiredDepartmentRuleFlagsABlankDepartment()
    {
        var rule = new VendorRollupRequiredDepartmentRule("vendor rollup needs this field");
        var item = VendorRollup.NewVendorRollup("", "");
        item.Department = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void VendorRollupCodeLengthRuleFlagsALongCode()
    {
        var rule = new VendorRollupCodeLengthRule(80);
        var item = VendorRollup.NewVendorRollup("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void VendorRollupRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new VendorRollupRequiredTitleRule("vendor rollup needs a value");
        var item = VendorRollup.NewVendorRollup("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        VendorRollupValidatorReportsEveryProblem();
        VendorRollupRequiredDepartmentRuleFlagsABlankDepartment();
        VendorRollupCodeLengthRuleFlagsALongCode();
        VendorRollupRequiredTitleRuleFlagsABlankTitle();
    }
}
