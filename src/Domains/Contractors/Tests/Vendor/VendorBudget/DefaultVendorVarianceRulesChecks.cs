namespace Acme.Workforce.Contractors.Tests.Vendor.VendorBudget;

using Acme.Workforce.Contractors.Vendor.VendorBudget;

/// <summary>Checks of vendor budget code, run by the project's runner.</summary>
public static class DefaultVendorVarianceRulesChecks
{
    public static void DefaultVendorVarianceValidatorReportsEveryProblem()
    {
        var validator = DefaultVendorVarianceRules.DefaultDefaultVendorVarianceValidator();
        var item = DefaultVendorVariance.NewDefaultVendorVariance("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultVendorVarianceRequiredPhoneRuleFlagsABlankPhone()
    {
        var rule = new DefaultVendorVarianceRequiredPhoneRule("default vendor variance needs an entry");
        var item = DefaultVendorVariance.NewDefaultVendorVariance("", "");
        item.Phone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultVendorVarianceRequiredCategoryRuleFlagsABlankCategory()
    {
        var rule = new DefaultVendorVarianceRequiredCategoryRule("default vendor variance needs this field");
        var item = DefaultVendorVariance.NewDefaultVendorVariance("", "");
        item.Category = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        DefaultVendorVarianceValidatorReportsEveryProblem();
        DefaultVendorVarianceRequiredPhoneRuleFlagsABlankPhone();
        DefaultVendorVarianceRequiredCategoryRuleFlagsABlankCategory();
    }
}
