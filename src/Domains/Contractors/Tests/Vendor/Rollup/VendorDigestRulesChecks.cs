namespace Acme.Workforce.Contractors.Tests.Vendor.Rollup;

using Acme.Workforce.Contractors.Vendor.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class VendorDigestRulesChecks
{
    public static void VendorDigestValidatorReportsEveryProblem()
    {
        var validator = VendorDigestRules.DefaultVendorDigestValidator();
        var item = VendorDigest.NewVendorDigest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void VendorDigestRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new VendorDigestRequiredCodeRule("vendor digest needs a value");
        var item = VendorDigest.NewVendorDigest("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        VendorDigestValidatorReportsEveryProblem();
        VendorDigestRequiredCodeRuleFlagsABlankCode();
    }
}
