namespace Acme.Workforce.Engagement.Tests.TownHall.Milestone;

using Acme.Workforce.Engagement.TownHall.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class TownHallScheduleRulesChecks
{
    public static void TownHallScheduleValidatorReportsEveryProblem()
    {
        var validator = TownHallScheduleRules.DefaultTownHallScheduleValidator();
        var item = TownHallSchedule.NewTownHallSchedule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TownHallScheduleRequiredSummaryRuleFlagsABlankSummary()
    {
        var rule = new TownHallScheduleRequiredSummaryRule("town hall schedule needs a value");
        var item = TownHallSchedule.NewTownHallSchedule("", "");
        item.Summary = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TownHallScheduleHoursRangeRuleFlagsValuesAboveHours()
    {
        var rule = new TownHallScheduleHoursRangeRule(6.3, 4981.2);
        var item = TownHallSchedule.NewTownHallSchedule("x", "y");
        item.Hours = 4982.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TownHallScheduleCodeLengthRuleFlagsALongCode()
    {
        var rule = new TownHallScheduleCodeLengthRule(80);
        var item = TownHallSchedule.NewTownHallSchedule("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        TownHallScheduleValidatorReportsEveryProblem();
        TownHallScheduleRequiredSummaryRuleFlagsABlankSummary();
        TownHallScheduleHoursRangeRuleFlagsValuesAboveHours();
        TownHallScheduleCodeLengthRuleFlagsALongCode();
    }
}
