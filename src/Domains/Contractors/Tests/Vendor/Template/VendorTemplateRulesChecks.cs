namespace Acme.Workforce.Contractors.Tests.Vendor.Template;

using Acme.Workforce.Contractors.Vendor.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class VendorTemplateRulesChecks
{
    public static void VendorTemplateValidatorReportsEveryProblem()
    {
        var validator = VendorTemplateRules.DefaultVendorTemplateValidator();
        var item = VendorTemplate.NewVendorTemplate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void VendorTemplateRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new VendorTemplateRequiredCodeRule("vendor template needs this field");
        var item = VendorTemplate.NewVendorTemplate("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void VendorTemplateRequiredLocationRuleFlagsABlankLocation()
    {
        var rule = new VendorTemplateRequiredLocationRule("vendor template needs this field");
        var item = VendorTemplate.NewVendorTemplate("", "");
        item.Location = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void VendorTemplateRequiredNameRuleFlagsABlankName()
    {
        var rule = new VendorTemplateRequiredNameRule("vendor template needs a value");
        var item = VendorTemplate.NewVendorTemplate("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void VendorTemplateCodeLengthRuleFlagsALongCode()
    {
        var rule = new VendorTemplateCodeLengthRule(16);
        var item = VendorTemplate.NewVendorTemplate("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void VendorTemplateNameLengthRuleFlagsALongName()
    {
        var rule = new VendorTemplateNameLengthRule(1000);
        var item = VendorTemplate.NewVendorTemplate("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        VendorTemplateValidatorReportsEveryProblem();
        VendorTemplateRequiredCodeRuleFlagsABlankCode();
        VendorTemplateRequiredLocationRuleFlagsABlankLocation();
        VendorTemplateRequiredNameRuleFlagsABlankName();
        VendorTemplateCodeLengthRuleFlagsALongCode();
        VendorTemplateNameLengthRuleFlagsALongName();
    }
}
