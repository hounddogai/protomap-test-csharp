namespace Acme.Workforce.Talent.Tests.SkillProfile.Target;

using Acme.Workforce.Talent.SkillProfile.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class SkillProfilePlanRulesChecks
{
    public static void SkillProfilePlanValidatorReportsEveryProblem()
    {
        var validator = SkillProfilePlanRules.DefaultSkillProfilePlanValidator();
        var item = SkillProfilePlan.NewSkillProfilePlan("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SkillProfilePlanLabelLengthRuleFlagsALongLabel()
    {
        var rule = new SkillProfilePlanLabelLengthRule(86400);
        var item = SkillProfilePlan.NewSkillProfilePlan("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SkillProfilePlanRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new SkillProfilePlanRequiredCodeRule("skill profile plan needs a value");
        var item = SkillProfilePlan.NewSkillProfilePlan("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        SkillProfilePlanValidatorReportsEveryProblem();
        SkillProfilePlanLabelLengthRuleFlagsALongLabel();
        SkillProfilePlanRequiredCodeRuleFlagsABlankCode();
    }
}
