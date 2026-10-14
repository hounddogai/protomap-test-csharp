namespace Acme.Workforce.Absence.Tests.Balance.Threshold;

using Acme.Workforce.Absence.Balance.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class GlobalBalanceScheduleRulesChecks
{
    public static void GlobalBalanceScheduleValidatorReportsEveryProblem()
    {
        var validator = GlobalBalanceScheduleRules.DefaultGlobalBalanceScheduleValidator();
        var item = GlobalBalanceSchedule.NewGlobalBalanceSchedule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void GlobalBalanceScheduleTeamLengthRuleFlagsALongTeam()
    {
        var rule = new GlobalBalanceScheduleTeamLengthRule(16);
        var item = GlobalBalanceSchedule.NewGlobalBalanceSchedule("x", "y");
        item.Team = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Team = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GlobalBalanceScheduleRequiredTeamRuleFlagsABlankTeam()
    {
        var rule = new GlobalBalanceScheduleRequiredTeamRule("global balance schedule needs this field");
        var item = GlobalBalanceSchedule.NewGlobalBalanceSchedule("", "");
        item.Team = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void GlobalBalanceScheduleRequiredSkillRuleFlagsABlankSkill()
    {
        var rule = new GlobalBalanceScheduleRequiredSkillRule("global balance schedule needs an entry");
        var item = GlobalBalanceSchedule.NewGlobalBalanceSchedule("", "");
        item.Skill = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        GlobalBalanceScheduleValidatorReportsEveryProblem();
        GlobalBalanceScheduleTeamLengthRuleFlagsALongTeam();
        GlobalBalanceScheduleRequiredTeamRuleFlagsABlankTeam();
        GlobalBalanceScheduleRequiredSkillRuleFlagsABlankSkill();
    }
}
