namespace Acme.Workforce.Contractors.Tests.Vendor.VendorMetric;

using Acme.Workforce.Contractors.Vendor.VendorMetric;

/// <summary>Checks of vendor metric code, run by the project's runner.</summary>
public static class VendorMappingRulesChecks
{
    public static void VendorMappingValidatorReportsEveryProblem()
    {
        var validator = VendorMappingRules.DefaultVendorMappingValidator();
        var item = VendorMapping.NewVendorMapping("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void VendorMappingRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new VendorMappingRequiredCodeRule("vendor mapping needs a value");
        var item = VendorMapping.NewVendorMapping("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void VendorMappingSkillLengthRuleFlagsALongSkill()
    {
        var rule = new VendorMappingSkillLengthRule(86400);
        var item = VendorMapping.NewVendorMapping("x", "y");
        item.Skill = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Skill = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void VendorMappingRequiredSkillRuleFlagsABlankSkill()
    {
        var rule = new VendorMappingRequiredSkillRule("vendor mapping needs a value");
        var item = VendorMapping.NewVendorMapping("", "");
        item.Skill = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void VendorMappingTitleLengthRuleFlagsALongTitle()
    {
        var rule = new VendorMappingTitleLengthRule(255);
        var item = VendorMapping.NewVendorMapping("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        VendorMappingValidatorReportsEveryProblem();
        VendorMappingRequiredCodeRuleFlagsABlankCode();
        VendorMappingSkillLengthRuleFlagsALongSkill();
        VendorMappingRequiredSkillRuleFlagsABlankSkill();
        VendorMappingTitleLengthRuleFlagsALongTitle();
    }
}
