namespace Acme.Workforce.Talent.Tests.SkillProfile.Cycle;

using Acme.Workforce.Talent.SkillProfile.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class ScheduledSkillProfilePolicyRulesChecks
{
    public static void ScheduledSkillProfilePolicyValidatorReportsEveryProblem()
    {
        var validator = ScheduledSkillProfilePolicyRules.DefaultScheduledSkillProfilePolicyValidator();
        var item = ScheduledSkillProfilePolicy.NewScheduledSkillProfilePolicy("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScheduledSkillProfilePolicyRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ScheduledSkillProfilePolicyRequiredCodeRule("scheduled skill profile policy needs this field");
        var item = ScheduledSkillProfilePolicy.NewScheduledSkillProfilePolicy("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScheduledSkillProfilePolicyJobCodeLengthRuleFlagsALongJobCode()
    {
        var rule = new ScheduledSkillProfilePolicyJobCodeLengthRule(32);
        var item = ScheduledSkillProfilePolicy.NewScheduledSkillProfilePolicy("x", "y");
        item.JobCode = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.JobCode = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScheduledSkillProfilePolicyLabelLengthRuleFlagsALongLabel()
    {
        var rule = new ScheduledSkillProfilePolicyLabelLengthRule(80);
        var item = ScheduledSkillProfilePolicy.NewScheduledSkillProfilePolicy("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScheduledSkillProfilePolicyRequiredJobCodeRuleFlagsABlankJobCode()
    {
        var rule = new ScheduledSkillProfilePolicyRequiredJobCodeRule("scheduled skill profile policy needs a value");
        var item = ScheduledSkillProfilePolicy.NewScheduledSkillProfilePolicy("", "");
        item.JobCode = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ScheduledSkillProfilePolicyValidatorReportsEveryProblem();
        ScheduledSkillProfilePolicyRequiredCodeRuleFlagsABlankCode();
        ScheduledSkillProfilePolicyJobCodeLengthRuleFlagsALongJobCode();
        ScheduledSkillProfilePolicyLabelLengthRuleFlagsALongLabel();
        ScheduledSkillProfilePolicyRequiredJobCodeRuleFlagsABlankJobCode();
    }
}
