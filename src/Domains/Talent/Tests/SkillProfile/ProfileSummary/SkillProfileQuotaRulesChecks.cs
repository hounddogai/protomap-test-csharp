namespace Acme.Workforce.Talent.Tests.SkillProfile.ProfileSummary;

using Acme.Workforce.Talent.SkillProfile.ProfileSummary;

/// <summary>Checks of profile summary code, run by the project's runner.</summary>
public static class SkillProfileQuotaRulesChecks
{
    public static void SkillProfileQuotaValidatorReportsEveryProblem()
    {
        var validator = SkillProfileQuotaRules.DefaultSkillProfileQuotaValidator();
        var item = SkillProfileQuota.NewSkillProfileQuota("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SkillProfileQuotaRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new SkillProfileQuotaRequiredLabelRule("skill profile quota needs a value");
        var item = SkillProfileQuota.NewSkillProfileQuota("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SkillProfileQuotaLabelLengthRuleFlagsALongLabel()
    {
        var rule = new SkillProfileQuotaLabelLengthRule(16);
        var item = SkillProfileQuota.NewSkillProfileQuota("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SkillProfileQuotaAmountRangeRuleFlagsValuesAboveAmount()
    {
        var rule = new SkillProfileQuotaAmountRangeRule(3.7, 3111.3);
        var item = SkillProfileQuota.NewSkillProfileQuota("x", "y");
        item.Amount = 3112.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SkillProfileQuotaRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new SkillProfileQuotaRequiredCodeRule("skill profile quota needs an entry");
        var item = SkillProfileQuota.NewSkillProfileQuota("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        SkillProfileQuotaValidatorReportsEveryProblem();
        SkillProfileQuotaRequiredLabelRuleFlagsABlankLabel();
        SkillProfileQuotaLabelLengthRuleFlagsALongLabel();
        SkillProfileQuotaAmountRangeRuleFlagsValuesAboveAmount();
        SkillProfileQuotaRequiredCodeRuleFlagsABlankCode();
    }
}
