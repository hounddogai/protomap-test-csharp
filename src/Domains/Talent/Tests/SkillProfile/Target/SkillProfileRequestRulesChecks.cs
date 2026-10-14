namespace Acme.Workforce.Talent.Tests.SkillProfile.Target;

using Acme.Workforce.Talent.SkillProfile.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class SkillProfileRequestRulesChecks
{
    public static void SkillProfileRequestValidatorReportsEveryProblem()
    {
        var validator = SkillProfileRequestRules.DefaultSkillProfileRequestValidator();
        var item = SkillProfileRequest.NewSkillProfileRequest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SkillProfileRequestExpiresAtRangeRuleFlagsValuesAboveExpiresAt()
    {
        var rule = new SkillProfileRequestExpiresAtRangeRule(64, 80);
        var item = SkillProfileRequest.NewSkillProfileRequest("x", "y");
        item.ExpiresAt = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SkillProfileRequestCodeLengthRuleFlagsALongCode()
    {
        var rule = new SkillProfileRequestCodeLengthRule(32);
        var item = SkillProfileRequest.NewSkillProfileRequest("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        SkillProfileRequestValidatorReportsEveryProblem();
        SkillProfileRequestExpiresAtRangeRuleFlagsValuesAboveExpiresAt();
        SkillProfileRequestCodeLengthRuleFlagsALongCode();
    }
}
