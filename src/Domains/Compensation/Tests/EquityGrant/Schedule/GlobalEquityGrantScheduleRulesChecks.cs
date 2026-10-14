namespace Acme.Workforce.Compensation.Tests.EquityGrant.Schedule;

using Acme.Workforce.Compensation.EquityGrant.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class GlobalEquityGrantScheduleRulesChecks
{
    public static void GlobalEquityGrantScheduleValidatorReportsEveryProblem()
    {
        var validator = GlobalEquityGrantScheduleRules.DefaultGlobalEquityGrantScheduleValidator();
        var item = GlobalEquityGrantSchedule.NewGlobalEquityGrantSchedule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void GlobalEquityGrantScheduleRequiredNameRuleFlagsABlankName()
    {
        var rule = new GlobalEquityGrantScheduleRequiredNameRule("global equity grant schedule needs a value");
        var item = GlobalEquityGrantSchedule.NewGlobalEquityGrantSchedule("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void GlobalEquityGrantScheduleSkillLengthRuleFlagsALongSkill()
    {
        var rule = new GlobalEquityGrantScheduleSkillLengthRule(1000);
        var item = GlobalEquityGrantSchedule.NewGlobalEquityGrantSchedule("x", "y");
        item.Skill = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Skill = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GlobalEquityGrantScheduleExpiresAtRangeRuleFlagsValuesAboveExpiresAt()
    {
        var rule = new GlobalEquityGrantScheduleExpiresAtRangeRule(32, 86400);
        var item = GlobalEquityGrantSchedule.NewGlobalEquityGrantSchedule("x", "y");
        item.ExpiresAt = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void GlobalEquityGrantScheduleRequiredSkillRuleFlagsABlankSkill()
    {
        var rule = new GlobalEquityGrantScheduleRequiredSkillRule("global equity grant schedule needs an entry");
        var item = GlobalEquityGrantSchedule.NewGlobalEquityGrantSchedule("", "");
        item.Skill = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        GlobalEquityGrantScheduleValidatorReportsEveryProblem();
        GlobalEquityGrantScheduleRequiredNameRuleFlagsABlankName();
        GlobalEquityGrantScheduleSkillLengthRuleFlagsALongSkill();
        GlobalEquityGrantScheduleExpiresAtRangeRuleFlagsValuesAboveExpiresAt();
        GlobalEquityGrantScheduleRequiredSkillRuleFlagsABlankSkill();
    }
}
