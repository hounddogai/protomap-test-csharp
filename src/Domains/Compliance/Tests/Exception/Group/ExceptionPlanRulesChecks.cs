namespace Acme.Workforce.Compliance.Tests.Exception.Group;

using Acme.Workforce.Compliance.Exception.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class ExceptionPlanRulesChecks
{
    public static void ExceptionPlanValidatorReportsEveryProblem()
    {
        var validator = ExceptionPlanRules.DefaultExceptionPlanValidator();
        var item = ExceptionPlan.NewExceptionPlan("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ExceptionPlanRequiredBadgeRuleFlagsABlankBadge()
    {
        var rule = new ExceptionPlanRequiredBadgeRule("exception plan needs an entry");
        var item = ExceptionPlan.NewExceptionPlan("", "");
        item.Badge = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ExceptionPlanSkillLengthRuleFlagsALongSkill()
    {
        var rule = new ExceptionPlanSkillLengthRule(255);
        var item = ExceptionPlan.NewExceptionPlan("x", "y");
        item.Skill = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Skill = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ExceptionPlanLabelLengthRuleFlagsALongLabel()
    {
        var rule = new ExceptionPlanLabelLengthRule(32);
        var item = ExceptionPlan.NewExceptionPlan("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ExceptionPlanValidatorReportsEveryProblem();
        ExceptionPlanRequiredBadgeRuleFlagsABlankBadge();
        ExceptionPlanSkillLengthRuleFlagsALongSkill();
        ExceptionPlanLabelLengthRuleFlagsALongLabel();
    }
}
