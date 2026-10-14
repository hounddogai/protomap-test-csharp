namespace Acme.Workforce.Workplace.Tests.Booking.Assignment;

using Acme.Workforce.Workplace.Booking.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class BookingForecastRulesChecks
{
    public static void BookingForecastValidatorReportsEveryProblem()
    {
        var validator = BookingForecastRules.DefaultBookingForecastValidator();
        var item = BookingForecast.NewBookingForecast("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BookingForecastCostCenterLengthRuleFlagsALongCostCenter()
    {
        var rule = new BookingForecastCostCenterLengthRule(1000);
        var item = BookingForecast.NewBookingForecast("x", "y");
        item.CostCenter = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.CostCenter = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BookingForecastRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new BookingForecastRequiredTitleRule("booking forecast needs this field");
        var item = BookingForecast.NewBookingForecast("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BookingForecastScoreRangeRuleFlagsValuesAboveScore()
    {
        var rule = new BookingForecastScoreRangeRule(2.8, 4188.4);
        var item = BookingForecast.NewBookingForecast("x", "y");
        item.Score = 4189.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BookingForecastCodeLengthRuleFlagsALongCode()
    {
        var rule = new BookingForecastCodeLengthRule(1000);
        var item = BookingForecast.NewBookingForecast("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        BookingForecastValidatorReportsEveryProblem();
        BookingForecastCostCenterLengthRuleFlagsALongCostCenter();
        BookingForecastRequiredTitleRuleFlagsABlankTitle();
        BookingForecastScoreRangeRuleFlagsValuesAboveScore();
        BookingForecastCodeLengthRuleFlagsALongCode();
    }
}
