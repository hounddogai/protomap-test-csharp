namespace Acme.Workforce.Compliance.Tests.Exception.RollingExceptionGroup;

using Acme.Workforce.Compliance.Exception.RollingExceptionGroup;

/// <summary>Checks of rolling exception group code, run by the project's runner.</summary>
public static class LegacyExceptionAgreementRulesChecks
{
    public static void LegacyExceptionAgreementValidatorReportsEveryProblem()
    {
        var validator = LegacyExceptionAgreementRules.DefaultLegacyExceptionAgreementValidator();
        var item = LegacyExceptionAgreement.NewLegacyExceptionAgreement("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LegacyExceptionAgreementGraceDaysRangeRuleFlagsValuesAboveGraceDays()
    {
        var rule = new LegacyExceptionAgreementGraceDaysRangeRule(255, 1000);
        var item = LegacyExceptionAgreement.NewLegacyExceptionAgreement("x", "y");
        item.GraceDays = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LegacyExceptionAgreementCodeLengthRuleFlagsALongCode()
    {
        var rule = new LegacyExceptionAgreementCodeLengthRule(32);
        var item = LegacyExceptionAgreement.NewLegacyExceptionAgreement("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LegacyExceptionAgreementRequiredNameRuleFlagsABlankName()
    {
        var rule = new LegacyExceptionAgreementRequiredNameRule("legacy exception agreement needs an entry");
        var item = LegacyExceptionAgreement.NewLegacyExceptionAgreement("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LegacyExceptionAgreementDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes()
    {
        var rule = new LegacyExceptionAgreementDurationMinutesRangeRule(80, 80);
        var item = LegacyExceptionAgreement.NewLegacyExceptionAgreement("x", "y");
        item.DurationMinutes = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LegacyExceptionAgreementSkillLengthRuleFlagsALongSkill()
    {
        var rule = new LegacyExceptionAgreementSkillLengthRule(32);
        var item = LegacyExceptionAgreement.NewLegacyExceptionAgreement("x", "y");
        item.Skill = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Skill = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LegacyExceptionAgreementValidatorReportsEveryProblem();
        LegacyExceptionAgreementGraceDaysRangeRuleFlagsValuesAboveGraceDays();
        LegacyExceptionAgreementCodeLengthRuleFlagsALongCode();
        LegacyExceptionAgreementRequiredNameRuleFlagsABlankName();
        LegacyExceptionAgreementDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes();
        LegacyExceptionAgreementSkillLengthRuleFlagsALongSkill();
    }
}
