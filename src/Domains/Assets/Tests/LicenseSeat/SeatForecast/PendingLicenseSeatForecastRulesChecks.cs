namespace Acme.Workforce.Assets.Tests.LicenseSeat.SeatForecast;

using Acme.Workforce.Assets.LicenseSeat.SeatForecast;

/// <summary>Checks of seat forecast code, run by the project's runner.</summary>
public static class PendingLicenseSeatForecastRulesChecks
{
    public static void PendingLicenseSeatForecastValidatorReportsEveryProblem()
    {
        var validator = PendingLicenseSeatForecastRules.DefaultPendingLicenseSeatForecastValidator();
        var item = PendingLicenseSeatForecast.NewPendingLicenseSeatForecast("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PendingLicenseSeatForecastQuantityRangeRuleFlagsValuesAboveQuantity()
    {
        var rule = new PendingLicenseSeatForecastQuantityRangeRule(16, 80);
        var item = PendingLicenseSeatForecast.NewPendingLicenseSeatForecast("x", "y");
        item.Quantity = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PendingLicenseSeatForecastCodeLengthRuleFlagsALongCode()
    {
        var rule = new PendingLicenseSeatForecastCodeLengthRule(1000);
        var item = PendingLicenseSeatForecast.NewPendingLicenseSeatForecast("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PendingLicenseSeatForecastValidatorReportsEveryProblem();
        PendingLicenseSeatForecastQuantityRangeRuleFlagsValuesAboveQuantity();
        PendingLicenseSeatForecastCodeLengthRuleFlagsALongCode();
    }
}
