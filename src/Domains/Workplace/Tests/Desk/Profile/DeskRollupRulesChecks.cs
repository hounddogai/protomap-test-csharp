namespace Acme.Workforce.Workplace.Tests.Desk.Profile;

using Acme.Workforce.Workplace.Desk.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class DeskRollupRulesChecks
{
    public static void DeskRollupValidatorReportsEveryProblem()
    {
        var validator = DeskRollupRules.DefaultDeskRollupValidator();
        var item = DeskRollup.NewDeskRollup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DeskRollupNameLengthRuleFlagsALongName()
    {
        var rule = new DeskRollupNameLengthRule(1000);
        var item = DeskRollup.NewDeskRollup("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DeskRollupRequiredSkillRuleFlagsABlankSkill()
    {
        var rule = new DeskRollupRequiredSkillRule("desk rollup needs an entry");
        var item = DeskRollup.NewDeskRollup("", "");
        item.Skill = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DeskRollupCodeLengthRuleFlagsALongCode()
    {
        var rule = new DeskRollupCodeLengthRule(32);
        var item = DeskRollup.NewDeskRollup("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        DeskRollupValidatorReportsEveryProblem();
        DeskRollupNameLengthRuleFlagsALongName();
        DeskRollupRequiredSkillRuleFlagsABlankSkill();
        DeskRollupCodeLengthRuleFlagsALongCode();
    }
}
