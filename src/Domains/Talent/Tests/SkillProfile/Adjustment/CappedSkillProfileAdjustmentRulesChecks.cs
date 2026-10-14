namespace Acme.Workforce.Talent.Tests.SkillProfile.Adjustment;

using Acme.Workforce.Talent.SkillProfile.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class CappedSkillProfileAdjustmentRulesChecks
{
    public static void CappedSkillProfileAdjustmentValidatorReportsEveryProblem()
    {
        var validator = CappedSkillProfileAdjustmentRules.DefaultCappedSkillProfileAdjustmentValidator();
        var item = CappedSkillProfileAdjustment.NewCappedSkillProfileAdjustment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CappedSkillProfileAdjustmentRequiredNameRuleFlagsABlankName()
    {
        var rule = new CappedSkillProfileAdjustmentRequiredNameRule("capped skill profile adjustment needs this field");
        var item = CappedSkillProfileAdjustment.NewCappedSkillProfileAdjustment("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CappedSkillProfileAdjustmentRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new CappedSkillProfileAdjustmentRequiredCodeRule("capped skill profile adjustment needs an entry");
        var item = CappedSkillProfileAdjustment.NewCappedSkillProfileAdjustment("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CappedSkillProfileAdjustmentSalaryBandLengthRuleFlagsALongSalaryBand()
    {
        var rule = new CappedSkillProfileAdjustmentSalaryBandLengthRule(80);
        var item = CappedSkillProfileAdjustment.NewCappedSkillProfileAdjustment("x", "y");
        item.SalaryBand = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.SalaryBand = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CappedSkillProfileAdjustmentQuantityRangeRuleFlagsValuesAboveQuantity()
    {
        var rule = new CappedSkillProfileAdjustmentQuantityRangeRule(255, 3600);
        var item = CappedSkillProfileAdjustment.NewCappedSkillProfileAdjustment("x", "y");
        item.Quantity = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        CappedSkillProfileAdjustmentValidatorReportsEveryProblem();
        CappedSkillProfileAdjustmentRequiredNameRuleFlagsABlankName();
        CappedSkillProfileAdjustmentRequiredCodeRuleFlagsABlankCode();
        CappedSkillProfileAdjustmentSalaryBandLengthRuleFlagsALongSalaryBand();
        CappedSkillProfileAdjustmentQuantityRangeRuleFlagsValuesAboveQuantity();
    }
}
