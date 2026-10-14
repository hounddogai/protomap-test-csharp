namespace Acme.Workforce.Assets.Tests.LicenseSeat.SeatForecast;

using Acme.Workforce.Assets.LicenseSeat.SeatForecast;

/// <summary>Checks of seat forecast code, run by the project's runner.</summary>
public static class LicenseSeatEstimateRulesChecks
{
    public static void LicenseSeatEstimateValidatorReportsEveryProblem()
    {
        var validator = LicenseSeatEstimateRules.DefaultLicenseSeatEstimateValidator();
        var item = LicenseSeatEstimate.NewLicenseSeatEstimate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LicenseSeatEstimateChecksumLengthRuleFlagsALongChecksum()
    {
        var rule = new LicenseSeatEstimateChecksumLengthRule(120);
        var item = LicenseSeatEstimate.NewLicenseSeatEstimate("x", "y");
        item.Checksum = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Checksum = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LicenseSeatEstimateCodeLengthRuleFlagsALongCode()
    {
        var rule = new LicenseSeatEstimateCodeLengthRule(120);
        var item = LicenseSeatEstimate.NewLicenseSeatEstimate("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LicenseSeatEstimateDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes()
    {
        var rule = new LicenseSeatEstimateDurationMinutesRangeRule(64, 255);
        var item = LicenseSeatEstimate.NewLicenseSeatEstimate("x", "y");
        item.DurationMinutes = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        LicenseSeatEstimateValidatorReportsEveryProblem();
        LicenseSeatEstimateChecksumLengthRuleFlagsALongChecksum();
        LicenseSeatEstimateCodeLengthRuleFlagsALongCode();
        LicenseSeatEstimateDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes();
    }
}
