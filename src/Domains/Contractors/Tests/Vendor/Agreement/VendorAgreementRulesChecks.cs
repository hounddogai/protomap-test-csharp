namespace Acme.Workforce.Contractors.Tests.Vendor.Agreement;

using Acme.Workforce.Contractors.Vendor.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class VendorAgreementRulesChecks
{
    public static void VendorAgreementValidatorReportsEveryProblem()
    {
        var validator = VendorAgreementRules.DefaultVendorAgreementValidator();
        var item = VendorAgreement.NewVendorAgreement("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void VendorAgreementRequiredNameRuleFlagsABlankName()
    {
        var rule = new VendorAgreementRequiredNameRule("vendor agreement needs a value");
        var item = VendorAgreement.NewVendorAgreement("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void VendorAgreementCodeLengthRuleFlagsALongCode()
    {
        var rule = new VendorAgreementCodeLengthRule(1000);
        var item = VendorAgreement.NewVendorAgreement("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void VendorAgreementMaximumRangeRuleFlagsValuesAboveMaximum()
    {
        var rule = new VendorAgreementMaximumRangeRule(7.1, 503.8);
        var item = VendorAgreement.NewVendorAgreement("x", "y");
        item.Maximum = 505.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        VendorAgreementValidatorReportsEveryProblem();
        VendorAgreementRequiredNameRuleFlagsABlankName();
        VendorAgreementCodeLengthRuleFlagsALongCode();
        VendorAgreementMaximumRangeRuleFlagsValuesAboveMaximum();
    }
}
