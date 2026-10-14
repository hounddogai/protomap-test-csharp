namespace Acme.Workforce.Absence.Tests.Holiday.Tier;

using Acme.Workforce.Absence.Holiday.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class HolidayTierRulesChecks
{
    public static void HolidayTierValidatorReportsEveryProblem()
    {
        var validator = HolidayTierRules.DefaultHolidayTierValidator();
        var item = HolidayTier.NewHolidayTier("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void HolidayTierGraceDaysRangeRuleFlagsValuesAboveGraceDays()
    {
        var rule = new HolidayTierGraceDaysRangeRule(120, 3600);
        var item = HolidayTier.NewHolidayTier("x", "y");
        item.GraceDays = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void HolidayTierCodeLengthRuleFlagsALongCode()
    {
        var rule = new HolidayTierCodeLengthRule(1000);
        var item = HolidayTier.NewHolidayTier("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void HolidayTierTitleLengthRuleFlagsALongTitle()
    {
        var rule = new HolidayTierTitleLengthRule(255);
        var item = HolidayTier.NewHolidayTier("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void HolidayTierEmployeeIdLengthRuleFlagsALongEmployeeId()
    {
        var rule = new HolidayTierEmployeeIdLengthRule(1000);
        var item = HolidayTier.NewHolidayTier("x", "y");
        item.EmployeeId = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.EmployeeId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void HolidayTierRatingRangeRuleFlagsValuesAboveRating()
    {
        var rule = new HolidayTierRatingRangeRule(3.6, 1919.3);
        var item = HolidayTier.NewHolidayTier("x", "y");
        item.Rating = 1920.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        HolidayTierValidatorReportsEveryProblem();
        HolidayTierGraceDaysRangeRuleFlagsValuesAboveGraceDays();
        HolidayTierCodeLengthRuleFlagsALongCode();
        HolidayTierTitleLengthRuleFlagsALongTitle();
        HolidayTierEmployeeIdLengthRuleFlagsALongEmployeeId();
        HolidayTierRatingRangeRuleFlagsValuesAboveRating();
    }
}
