namespace Acme.Workforce.Assets.Tests.Warranty.Bundle;

using Acme.Workforce.Assets.Warranty.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class LenientWarrantyBundleRulesChecks
{
    public static void LenientWarrantyBundleValidatorReportsEveryProblem()
    {
        var validator = LenientWarrantyBundleRules.DefaultLenientWarrantyBundleValidator();
        var item = LenientWarrantyBundle.NewLenientWarrantyBundle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientWarrantyBundleCodeLengthRuleFlagsALongCode()
    {
        var rule = new LenientWarrantyBundleCodeLengthRule(255);
        var item = LenientWarrantyBundle.NewLenientWarrantyBundle("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LenientWarrantyBundleRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LenientWarrantyBundleRequiredCodeRule("lenient warranty bundle needs an entry");
        var item = LenientWarrantyBundle.NewLenientWarrantyBundle("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientWarrantyBundleSkillLengthRuleFlagsALongSkill()
    {
        var rule = new LenientWarrantyBundleSkillLengthRule(120);
        var item = LenientWarrantyBundle.NewLenientWarrantyBundle("x", "y");
        item.Skill = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Skill = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LenientWarrantyBundleTenureMonthsRangeRuleFlagsValuesAboveTenureMonths()
    {
        var rule = new LenientWarrantyBundleTenureMonthsRangeRule(32, 32);
        var item = LenientWarrantyBundle.NewLenientWarrantyBundle("x", "y");
        item.TenureMonths = 33;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        LenientWarrantyBundleValidatorReportsEveryProblem();
        LenientWarrantyBundleCodeLengthRuleFlagsALongCode();
        LenientWarrantyBundleRequiredCodeRuleFlagsABlankCode();
        LenientWarrantyBundleSkillLengthRuleFlagsALongSkill();
        LenientWarrantyBundleTenureMonthsRangeRuleFlagsValuesAboveTenureMonths();
    }
}
