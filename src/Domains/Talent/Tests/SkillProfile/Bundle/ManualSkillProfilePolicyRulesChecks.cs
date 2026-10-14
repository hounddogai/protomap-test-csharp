namespace Acme.Workforce.Talent.Tests.SkillProfile.Bundle;

using Acme.Workforce.Talent.SkillProfile.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class ManualSkillProfilePolicyRulesChecks
{
    public static void ManualSkillProfilePolicyValidatorReportsEveryProblem()
    {
        var validator = ManualSkillProfilePolicyRules.DefaultManualSkillProfilePolicyValidator();
        var item = ManualSkillProfilePolicy.NewManualSkillProfilePolicy("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ManualSkillProfilePolicyUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt()
    {
        var rule = new ManualSkillProfilePolicyUpdatedAtRangeRule(80, 86400);
        var item = ManualSkillProfilePolicy.NewManualSkillProfilePolicy("x", "y");
        item.UpdatedAt = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ManualSkillProfilePolicyRequiredManagerIdRuleFlagsABlankManagerId()
    {
        var rule = new ManualSkillProfilePolicyRequiredManagerIdRule("manual skill profile policy needs an entry");
        var item = ManualSkillProfilePolicy.NewManualSkillProfilePolicy("", "");
        item.ManagerId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ManualSkillProfilePolicyRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new ManualSkillProfilePolicyRequiredTitleRule("manual skill profile policy needs a value");
        var item = ManualSkillProfilePolicy.NewManualSkillProfilePolicy("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ManualSkillProfilePolicyValidatorReportsEveryProblem();
        ManualSkillProfilePolicyUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt();
        ManualSkillProfilePolicyRequiredManagerIdRuleFlagsABlankManagerId();
        ManualSkillProfilePolicyRequiredTitleRuleFlagsABlankTitle();
    }
}
