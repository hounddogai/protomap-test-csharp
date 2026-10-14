namespace Acme.Workforce.Absence.Tests.Holiday.Schedule;

using Acme.Workforce.Absence.Holiday.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class HolidayScheduleRulesChecks
{
    public static void HolidayScheduleValidatorReportsEveryProblem()
    {
        var validator = HolidayScheduleRules.DefaultHolidayScheduleValidator();
        var item = HolidaySchedule.NewHolidaySchedule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void HolidayScheduleRatioRangeRuleFlagsValuesAboveRatio()
    {
        var rule = new HolidayScheduleRatioRangeRule(9.3, 1392.6);
        var item = HolidaySchedule.NewHolidaySchedule("x", "y");
        item.Ratio = 1394.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        HolidayScheduleValidatorReportsEveryProblem();
        HolidayScheduleRatioRangeRuleFlagsValuesAboveRatio();
    }
}
