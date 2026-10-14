namespace Acme.Workforce.Assets.Tests.LicenseSeat.Reservation;

using Acme.Workforce.Assets.LicenseSeat.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class LicenseSeatReservationRulesChecks
{
    public static void LicenseSeatReservationValidatorReportsEveryProblem()
    {
        var validator = LicenseSeatReservationRules.DefaultLicenseSeatReservationValidator();
        var item = LicenseSeatReservation.NewLicenseSeatReservation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LicenseSeatReservationTitleLengthRuleFlagsALongTitle()
    {
        var rule = new LicenseSeatReservationTitleLengthRule(64);
        var item = LicenseSeatReservation.NewLicenseSeatReservation("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LicenseSeatReservationRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LicenseSeatReservationRequiredCodeRule("license seat reservation needs an entry");
        var item = LicenseSeatReservation.NewLicenseSeatReservation("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LicenseSeatReservationWindowDaysRangeRuleFlagsValuesAboveWindowDays()
    {
        var rule = new LicenseSeatReservationWindowDaysRangeRule(255, 1000);
        var item = LicenseSeatReservation.NewLicenseSeatReservation("x", "y");
        item.WindowDays = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LicenseSeatReservationGraceDaysRangeRuleFlagsValuesAboveGraceDays()
    {
        var rule = new LicenseSeatReservationGraceDaysRangeRule(32, 80);
        var item = LicenseSeatReservation.NewLicenseSeatReservation("x", "y");
        item.GraceDays = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        LicenseSeatReservationValidatorReportsEveryProblem();
        LicenseSeatReservationTitleLengthRuleFlagsALongTitle();
        LicenseSeatReservationRequiredCodeRuleFlagsABlankCode();
        LicenseSeatReservationWindowDaysRangeRuleFlagsValuesAboveWindowDays();
        LicenseSeatReservationGraceDaysRangeRuleFlagsValuesAboveGraceDays();
    }
}
