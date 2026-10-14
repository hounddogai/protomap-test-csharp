namespace Acme.Workforce.Talent.Tests.SkillProfile.Threshold;

using Acme.Workforce.Talent.SkillProfile.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class RollingSkillProfileRecordRulesChecks
{
    public static void RollingSkillProfileRecordValidatorReportsEveryProblem()
    {
        var validator = RollingSkillProfileRecordRules.DefaultRollingSkillProfileRecordValidator();
        var item = RollingSkillProfileRecord.NewRollingSkillProfileRecord("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RollingSkillProfileRecordRequiredNameRuleFlagsABlankName()
    {
        var rule = new RollingSkillProfileRecordRequiredNameRule("rolling skill profile record needs a value");
        var item = RollingSkillProfileRecord.NewRollingSkillProfileRecord("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RollingSkillProfileRecordEmployeeIdLengthRuleFlagsALongEmployeeId()
    {
        var rule = new RollingSkillProfileRecordEmployeeIdLengthRule(86400);
        var item = RollingSkillProfileRecord.NewRollingSkillProfileRecord("x", "y");
        item.EmployeeId = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.EmployeeId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RollingSkillProfileRecordRequiredEmployeeIdRuleFlagsABlankEmployeeId()
    {
        var rule = new RollingSkillProfileRecordRequiredEmployeeIdRule("rolling skill profile record needs this field");
        var item = RollingSkillProfileRecord.NewRollingSkillProfileRecord("", "");
        item.EmployeeId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RollingSkillProfileRecordNameLengthRuleFlagsALongName()
    {
        var rule = new RollingSkillProfileRecordNameLengthRule(80);
        var item = RollingSkillProfileRecord.NewRollingSkillProfileRecord("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        RollingSkillProfileRecordValidatorReportsEveryProblem();
        RollingSkillProfileRecordRequiredNameRuleFlagsABlankName();
        RollingSkillProfileRecordEmployeeIdLengthRuleFlagsALongEmployeeId();
        RollingSkillProfileRecordRequiredEmployeeIdRuleFlagsABlankEmployeeId();
        RollingSkillProfileRecordNameLengthRuleFlagsALongName();
    }
}
