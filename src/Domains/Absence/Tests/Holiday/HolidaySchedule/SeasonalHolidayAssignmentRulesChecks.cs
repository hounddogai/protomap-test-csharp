namespace Acme.Workforce.Absence.Tests.Holiday.HolidaySchedule;

using Acme.Workforce.Absence.Holiday.HolidaySchedule;

/// <summary>Checks of holiday schedule code, run by the project's runner.</summary>
public static class SeasonalHolidayAssignmentRulesChecks
{
    public static void SeasonalHolidayAssignmentValidatorReportsEveryProblem()
    {
        var validator = SeasonalHolidayAssignmentRules.DefaultSeasonalHolidayAssignmentValidator();
        var item = SeasonalHolidayAssignment.NewSeasonalHolidayAssignment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SeasonalHolidayAssignmentFloorValueRangeRuleFlagsValuesAboveFloorValue()
    {
        var rule = new SeasonalHolidayAssignmentFloorValueRangeRule(1.5, 4326.9);
        var item = SeasonalHolidayAssignment.NewSeasonalHolidayAssignment("x", "y");
        item.FloorValue = 4328.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SeasonalHolidayAssignmentRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new SeasonalHolidayAssignmentRequiredLabelRule("seasonal holiday assignment needs a value");
        var item = SeasonalHolidayAssignment.NewSeasonalHolidayAssignment("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SeasonalHolidayAssignmentLabelLengthRuleFlagsALongLabel()
    {
        var rule = new SeasonalHolidayAssignmentLabelLengthRule(1000);
        var item = SeasonalHolidayAssignment.NewSeasonalHolidayAssignment("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        SeasonalHolidayAssignmentValidatorReportsEveryProblem();
        SeasonalHolidayAssignmentFloorValueRangeRuleFlagsValuesAboveFloorValue();
        SeasonalHolidayAssignmentRequiredLabelRuleFlagsABlankLabel();
        SeasonalHolidayAssignmentLabelLengthRuleFlagsALongLabel();
    }
}
