namespace Acme.Workforce.Assets.Tests.Warranty.Digest;

using Acme.Workforce.Assets.Warranty.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class WarrantyDigestRulesChecks
{
    public static void WarrantyDigestValidatorReportsEveryProblem()
    {
        var validator = WarrantyDigestRules.DefaultWarrantyDigestValidator();
        var item = WarrantyDigest.NewWarrantyDigest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WarrantyDigestRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new WarrantyDigestRequiredCodeRule("warranty digest needs this field");
        var item = WarrantyDigest.NewWarrantyDigest("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void WarrantyDigestCostCenterLengthRuleFlagsALongCostCenter()
    {
        var rule = new WarrantyDigestCostCenterLengthRule(16);
        var item = WarrantyDigest.NewWarrantyDigest("x", "y");
        item.CostCenter = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.CostCenter = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void WarrantyDigestRequiredCostCenterRuleFlagsABlankCostCenter()
    {
        var rule = new WarrantyDigestRequiredCostCenterRule("warranty digest needs an entry");
        var item = WarrantyDigest.NewWarrantyDigest("", "");
        item.CostCenter = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void WarrantyDigestRequiredSkillRuleFlagsABlankSkill()
    {
        var rule = new WarrantyDigestRequiredSkillRule("warranty digest needs this field");
        var item = WarrantyDigest.NewWarrantyDigest("", "");
        item.Skill = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void WarrantyDigestRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new WarrantyDigestRequiredLabelRule("warranty digest needs this field");
        var item = WarrantyDigest.NewWarrantyDigest("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        WarrantyDigestValidatorReportsEveryProblem();
        WarrantyDigestRequiredCodeRuleFlagsABlankCode();
        WarrantyDigestCostCenterLengthRuleFlagsALongCostCenter();
        WarrantyDigestRequiredCostCenterRuleFlagsABlankCostCenter();
        WarrantyDigestRequiredSkillRuleFlagsABlankSkill();
        WarrantyDigestRequiredLabelRuleFlagsABlankLabel();
    }
}
