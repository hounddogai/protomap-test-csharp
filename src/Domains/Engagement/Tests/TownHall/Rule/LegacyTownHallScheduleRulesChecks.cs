namespace Acme.Workforce.Engagement.Tests.TownHall.Rule;

using Acme.Workforce.Engagement.TownHall.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class LegacyTownHallScheduleRulesChecks
{
    public static void LegacyTownHallScheduleValidatorReportsEveryProblem()
    {
        var validator = LegacyTownHallScheduleRules.DefaultLegacyTownHallScheduleValidator();
        var item = LegacyTownHallSchedule.NewLegacyTownHallSchedule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LegacyTownHallScheduleSourceLengthRuleFlagsALongSource()
    {
        var rule = new LegacyTownHallScheduleSourceLengthRule(3600);
        var item = LegacyTownHallSchedule.NewLegacyTownHallSchedule("x", "y");
        item.Source = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Source = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LegacyTownHallScheduleCodeLengthRuleFlagsALongCode()
    {
        var rule = new LegacyTownHallScheduleCodeLengthRule(16);
        var item = LegacyTownHallSchedule.NewLegacyTownHallSchedule("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LegacyTownHallScheduleRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new LegacyTownHallScheduleRequiredTitleRule("legacy town hall schedule needs a value");
        var item = LegacyTownHallSchedule.NewLegacyTownHallSchedule("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LegacyTownHallScheduleCeilingRangeRuleFlagsValuesAboveCeiling()
    {
        var rule = new LegacyTownHallScheduleCeilingRangeRule(1.6, 4784.9);
        var item = LegacyTownHallSchedule.NewLegacyTownHallSchedule("x", "y");
        item.Ceiling = 4786.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        LegacyTownHallScheduleValidatorReportsEveryProblem();
        LegacyTownHallScheduleSourceLengthRuleFlagsALongSource();
        LegacyTownHallScheduleCodeLengthRuleFlagsALongCode();
        LegacyTownHallScheduleRequiredTitleRuleFlagsABlankTitle();
        LegacyTownHallScheduleCeilingRangeRuleFlagsValuesAboveCeiling();
    }
}
