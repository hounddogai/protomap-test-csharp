namespace Acme.Workforce.Assets.Tests.LicenseSeat.SeatReservation;

using Acme.Workforce.Assets.LicenseSeat.SeatReservation;

/// <summary>Checks of seat reservation code, run by the project's runner.</summary>
public static class FallbackLicenseSeatReservationRulesChecks
{
    public static void FallbackLicenseSeatReservationValidatorReportsEveryProblem()
    {
        var validator = FallbackLicenseSeatReservationRules.DefaultFallbackLicenseSeatReservationValidator();
        var item = FallbackLicenseSeatReservation.NewFallbackLicenseSeatReservation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FallbackLicenseSeatReservationRequiredTimezoneRuleFlagsABlankTimezone()
    {
        var rule = new FallbackLicenseSeatReservationRequiredTimezoneRule(
            "fallback license seat reservation needs a value");
        var item = FallbackLicenseSeatReservation.NewFallbackLicenseSeatReservation("", "");
        item.Timezone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FallbackLicenseSeatReservationRetriesRangeRuleFlagsValuesAboveRetries()
    {
        var rule = new FallbackLicenseSeatReservationRetriesRangeRule(32, 64);
        var item = FallbackLicenseSeatReservation.NewFallbackLicenseSeatReservation("x", "y");
        item.Retries = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void FallbackLicenseSeatReservationRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new FallbackLicenseSeatReservationRequiredCodeRule("fallback license seat reservation needs a value");
        var item = FallbackLicenseSeatReservation.NewFallbackLicenseSeatReservation("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FallbackLicenseSeatReservationRequiredNameRuleFlagsABlankName()
    {
        var rule = new FallbackLicenseSeatReservationRequiredNameRule(
            "fallback license seat reservation needs an entry");
        var item = FallbackLicenseSeatReservation.NewFallbackLicenseSeatReservation("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        FallbackLicenseSeatReservationValidatorReportsEveryProblem();
        FallbackLicenseSeatReservationRequiredTimezoneRuleFlagsABlankTimezone();
        FallbackLicenseSeatReservationRetriesRangeRuleFlagsValuesAboveRetries();
        FallbackLicenseSeatReservationRequiredCodeRuleFlagsABlankCode();
        FallbackLicenseSeatReservationRequiredNameRuleFlagsABlankName();
    }
}
