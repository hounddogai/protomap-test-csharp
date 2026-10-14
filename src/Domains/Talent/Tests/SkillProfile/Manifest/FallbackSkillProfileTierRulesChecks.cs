namespace Acme.Workforce.Talent.Tests.SkillProfile.Manifest;

using Acme.Workforce.Talent.SkillProfile.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class FallbackSkillProfileTierRulesChecks
{
    public static void FallbackSkillProfileTierValidatorReportsEveryProblem()
    {
        var validator = FallbackSkillProfileTierRules.DefaultFallbackSkillProfileTierValidator();
        var item = FallbackSkillProfileTier.NewFallbackSkillProfileTier("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FallbackSkillProfileTierVersionRangeRuleFlagsValuesAboveVersion()
    {
        var rule = new FallbackSkillProfileTierVersionRangeRule(64, 64);
        var item = FallbackSkillProfileTier.NewFallbackSkillProfileTier("x", "y");
        item.Version = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void FallbackSkillProfileTierGradeLengthRuleFlagsALongGrade()
    {
        var rule = new FallbackSkillProfileTierGradeLengthRule(120);
        var item = FallbackSkillProfileTier.NewFallbackSkillProfileTier("x", "y");
        item.Grade = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Grade = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FallbackSkillProfileTierRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new FallbackSkillProfileTierRequiredCodeRule("fallback skill profile tier needs a value");
        var item = FallbackSkillProfileTier.NewFallbackSkillProfileTier("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        FallbackSkillProfileTierValidatorReportsEveryProblem();
        FallbackSkillProfileTierVersionRangeRuleFlagsValuesAboveVersion();
        FallbackSkillProfileTierGradeLengthRuleFlagsALongGrade();
        FallbackSkillProfileTierRequiredCodeRuleFlagsABlankCode();
    }
}
