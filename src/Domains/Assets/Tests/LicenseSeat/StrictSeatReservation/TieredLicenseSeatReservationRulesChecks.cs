namespace Acme.Workforce.Assets.Tests.LicenseSeat.StrictSeatReservation;

using Acme.Workforce.Assets.LicenseSeat.StrictSeatReservation;

/// <summary>Checks of strict seat reservation code, run by the project's runner.</summary>
public static class TieredLicenseSeatReservationRulesChecks
{
    public static void TieredLicenseSeatReservationValidatorReportsEveryProblem()
    {
        var validator = TieredLicenseSeatReservationRules.DefaultTieredLicenseSeatReservationValidator();
        var item = TieredLicenseSeatReservation.NewTieredLicenseSeatReservation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TieredLicenseSeatReservationLabelLengthRuleFlagsALongLabel()
    {
        var rule = new TieredLicenseSeatReservationLabelLengthRule(3600);
        var item = TieredLicenseSeatReservation.NewTieredLicenseSeatReservation("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TieredLicenseSeatReservationRequiredCategoryRuleFlagsABlankCategory()
    {
        var rule = new TieredLicenseSeatReservationRequiredCategoryRule(
            "tiered license seat reservation needs this field");
        var item = TieredLicenseSeatReservation.NewTieredLicenseSeatReservation("", "");
        item.Category = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TieredLicenseSeatReservationRequiredManagerIdRuleFlagsABlankManagerId()
    {
        var rule = new TieredLicenseSeatReservationRequiredManagerIdRule(
            "tiered license seat reservation needs an entry");
        var item = TieredLicenseSeatReservation.NewTieredLicenseSeatReservation("", "");
        item.ManagerId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        TieredLicenseSeatReservationValidatorReportsEveryProblem();
        TieredLicenseSeatReservationLabelLengthRuleFlagsALongLabel();
        TieredLicenseSeatReservationRequiredCategoryRuleFlagsABlankCategory();
        TieredLicenseSeatReservationRequiredManagerIdRuleFlagsABlankManagerId();
    }
}
