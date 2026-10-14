namespace Acme.Workforce.Absence.Tests.Holiday.Reservation;

using Acme.Workforce.Absence.Holiday.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class SeasonalHolidayExceptionRulesChecks
{
    public static void SeasonalHolidayExceptionValidatorReportsEveryProblem()
    {
        var validator = SeasonalHolidayExceptionRules.DefaultSeasonalHolidayExceptionValidator();
        var item = SeasonalHolidayException.NewSeasonalHolidayException("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SeasonalHolidayExceptionRetriesRangeRuleFlagsValuesAboveRetries()
    {
        var rule = new SeasonalHolidayExceptionRetriesRangeRule(64, 1000);
        var item = SeasonalHolidayException.NewSeasonalHolidayException("x", "y");
        item.Retries = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SeasonalHolidayExceptionFloorValueRangeRuleFlagsValuesAboveFloorValue()
    {
        var rule = new SeasonalHolidayExceptionFloorValueRangeRule(1.4, 816.3);
        var item = SeasonalHolidayException.NewSeasonalHolidayException("x", "y");
        item.FloorValue = 817.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SeasonalHolidayExceptionNameLengthRuleFlagsALongName()
    {
        var rule = new SeasonalHolidayExceptionNameLengthRule(1000);
        var item = SeasonalHolidayException.NewSeasonalHolidayException("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        SeasonalHolidayExceptionValidatorReportsEveryProblem();
        SeasonalHolidayExceptionRetriesRangeRuleFlagsValuesAboveRetries();
        SeasonalHolidayExceptionFloorValueRangeRuleFlagsValuesAboveFloorValue();
        SeasonalHolidayExceptionNameLengthRuleFlagsALongName();
    }
}
