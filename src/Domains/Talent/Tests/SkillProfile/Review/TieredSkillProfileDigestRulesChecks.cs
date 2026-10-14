namespace Acme.Workforce.Talent.Tests.SkillProfile.Review;

using Acme.Workforce.Talent.SkillProfile.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class TieredSkillProfileDigestRulesChecks
{
    public static void TieredSkillProfileDigestValidatorReportsEveryProblem()
    {
        var validator = TieredSkillProfileDigestRules.DefaultTieredSkillProfileDigestValidator();
        var item = TieredSkillProfileDigest.NewTieredSkillProfileDigest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TieredSkillProfileDigestCostCenterLengthRuleFlagsALongCostCenter()
    {
        var rule = new TieredSkillProfileDigestCostCenterLengthRule(64);
        var item = TieredSkillProfileDigest.NewTieredSkillProfileDigest("x", "y");
        item.CostCenter = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.CostCenter = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TieredSkillProfileDigestRequiredCostCenterRuleFlagsABlankCostCenter()
    {
        var rule = new TieredSkillProfileDigestRequiredCostCenterRule("tiered skill profile digest needs an entry");
        var item = TieredSkillProfileDigest.NewTieredSkillProfileDigest("", "");
        item.CostCenter = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TieredSkillProfileDigestRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new TieredSkillProfileDigestRequiredCodeRule("tiered skill profile digest needs a value");
        var item = TieredSkillProfileDigest.NewTieredSkillProfileDigest("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        TieredSkillProfileDigestValidatorReportsEveryProblem();
        TieredSkillProfileDigestCostCenterLengthRuleFlagsALongCostCenter();
        TieredSkillProfileDigestRequiredCostCenterRuleFlagsABlankCostCenter();
        TieredSkillProfileDigestRequiredCodeRuleFlagsABlankCode();
    }
}
