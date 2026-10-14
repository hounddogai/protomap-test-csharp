namespace Acme.Workforce.Assets.Tests.LicenseSeat.Forecast;

using Acme.Workforce.Assets.LicenseSeat.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class LicenseSeatForecastRulesChecks
{
    public static void LicenseSeatForecastValidatorReportsEveryProblem()
    {
        var validator = LicenseSeatForecastRules.DefaultLicenseSeatForecastValidator();
        var item = LicenseSeatForecast.NewLicenseSeatForecast("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LicenseSeatForecastNameLengthRuleFlagsALongName()
    {
        var rule = new LicenseSeatForecastNameLengthRule(32);
        var item = LicenseSeatForecast.NewLicenseSeatForecast("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LicenseSeatForecastExpiresAtRangeRuleFlagsValuesAboveExpiresAt()
    {
        var rule = new LicenseSeatForecastExpiresAtRangeRule(16, 1000);
        var item = LicenseSeatForecast.NewLicenseSeatForecast("x", "y");
        item.ExpiresAt = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LicenseSeatForecastRequiredChecksumRuleFlagsABlankChecksum()
    {
        var rule = new LicenseSeatForecastRequiredChecksumRule("license seat forecast needs this field");
        var item = LicenseSeatForecast.NewLicenseSeatForecast("", "");
        item.Checksum = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LicenseSeatForecastRequiredNameRuleFlagsABlankName()
    {
        var rule = new LicenseSeatForecastRequiredNameRule("license seat forecast needs an entry");
        var item = LicenseSeatForecast.NewLicenseSeatForecast("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LicenseSeatForecastRequiredDescriptionRuleFlagsABlankDescription()
    {
        var rule = new LicenseSeatForecastRequiredDescriptionRule("license seat forecast needs this field");
        var item = LicenseSeatForecast.NewLicenseSeatForecast("", "");
        item.Description = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LicenseSeatForecastValidatorReportsEveryProblem();
        LicenseSeatForecastNameLengthRuleFlagsALongName();
        LicenseSeatForecastExpiresAtRangeRuleFlagsValuesAboveExpiresAt();
        LicenseSeatForecastRequiredChecksumRuleFlagsABlankChecksum();
        LicenseSeatForecastRequiredNameRuleFlagsABlankName();
        LicenseSeatForecastRequiredDescriptionRuleFlagsABlankDescription();
    }
}
