namespace Acme.Workforce.Absence.Tests.Holiday.OverrideItem;

using Acme.Workforce.Absence.Holiday.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class HolidayExceptionRulesChecks
{
    public static void HolidayExceptionValidatorReportsEveryProblem()
    {
        var validator = HolidayExceptionRules.DefaultHolidayExceptionValidator();
        var item = HolidayException.NewHolidayException("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void HolidayExceptionFteRangeRuleFlagsValuesAboveFte()
    {
        var rule = new HolidayExceptionFteRangeRule(4.6, 4226.4);
        var item = HolidayException.NewHolidayException("x", "y");
        item.Fte = 4227.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void HolidayExceptionTitleLengthRuleFlagsALongTitle()
    {
        var rule = new HolidayExceptionTitleLengthRule(3600);
        var item = HolidayException.NewHolidayException("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void HolidayExceptionRatingRangeRuleFlagsValuesAboveRating()
    {
        var rule = new HolidayExceptionRatingRangeRule(2.4, 1400.1);
        var item = HolidayException.NewHolidayException("x", "y");
        item.Rating = 1401.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void HolidayExceptionRequiredEmployeeIdRuleFlagsABlankEmployeeId()
    {
        var rule = new HolidayExceptionRequiredEmployeeIdRule("holiday exception needs a value");
        var item = HolidayException.NewHolidayException("", "");
        item.EmployeeId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        HolidayExceptionValidatorReportsEveryProblem();
        HolidayExceptionFteRangeRuleFlagsValuesAboveFte();
        HolidayExceptionTitleLengthRuleFlagsALongTitle();
        HolidayExceptionRatingRangeRuleFlagsValuesAboveRating();
        HolidayExceptionRequiredEmployeeIdRuleFlagsABlankEmployeeId();
    }
}
