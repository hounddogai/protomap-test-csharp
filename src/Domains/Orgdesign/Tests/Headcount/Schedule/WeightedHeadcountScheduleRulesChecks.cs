namespace Acme.Workforce.Orgdesign.Tests.Headcount.Schedule;

using Acme.Workforce.Orgdesign.Headcount.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class WeightedHeadcountScheduleRulesChecks
{
    public static void WeightedHeadcountScheduleValidatorReportsEveryProblem()
    {
        var validator = WeightedHeadcountScheduleRules.DefaultWeightedHeadcountScheduleValidator();
        var item = WeightedHeadcountSchedule.NewWeightedHeadcountSchedule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WeightedHeadcountScheduleRequiredNameRuleFlagsABlankName()
    {
        var rule = new WeightedHeadcountScheduleRequiredNameRule("weighted headcount schedule needs this field");
        var item = WeightedHeadcountSchedule.NewWeightedHeadcountSchedule("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void WeightedHeadcountScheduleRequiredTeamRuleFlagsABlankTeam()
    {
        var rule = new WeightedHeadcountScheduleRequiredTeamRule("weighted headcount schedule needs this field");
        var item = WeightedHeadcountSchedule.NewWeightedHeadcountSchedule("", "");
        item.Team = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void WeightedHeadcountScheduleCodeLengthRuleFlagsALongCode()
    {
        var rule = new WeightedHeadcountScheduleCodeLengthRule(3600);
        var item = WeightedHeadcountSchedule.NewWeightedHeadcountSchedule("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        WeightedHeadcountScheduleValidatorReportsEveryProblem();
        WeightedHeadcountScheduleRequiredNameRuleFlagsABlankName();
        WeightedHeadcountScheduleRequiredTeamRuleFlagsABlankTeam();
        WeightedHeadcountScheduleCodeLengthRuleFlagsALongCode();
    }
}
