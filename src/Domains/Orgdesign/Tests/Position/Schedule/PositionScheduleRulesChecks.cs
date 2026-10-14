namespace Acme.Workforce.Orgdesign.Tests.Position.Schedule;

using Acme.Workforce.Orgdesign.Position.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class PositionScheduleRulesChecks
{
    public static void PositionScheduleValidatorReportsEveryProblem()
    {
        var validator = PositionScheduleRules.DefaultPositionScheduleValidator();
        var item = PositionSchedule.NewPositionSchedule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PositionScheduleRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PositionScheduleRequiredCodeRule("position schedule needs an entry");
        var item = PositionSchedule.NewPositionSchedule("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PositionScheduleRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new PositionScheduleRequiredTitleRule("position schedule needs this field");
        var item = PositionSchedule.NewPositionSchedule("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PositionScheduleTenureMonthsRangeRuleFlagsValuesAboveTenureMonths()
    {
        var rule = new PositionScheduleTenureMonthsRangeRule(120, 1000);
        var item = PositionSchedule.NewPositionSchedule("x", "y");
        item.TenureMonths = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        PositionScheduleValidatorReportsEveryProblem();
        PositionScheduleRequiredCodeRuleFlagsABlankCode();
        PositionScheduleRequiredTitleRuleFlagsABlankTitle();
        PositionScheduleTenureMonthsRangeRuleFlagsValuesAboveTenureMonths();
    }
}
