namespace Acme.Workforce.Talent.Tests.Mobility.RegionalMobilityReservation;

using Acme.Workforce.Talent.Mobility.RegionalMobilityReservation;

/// <summary>Checks of regional mobility reservation code, run by the project's runner.</summary>
public static class TieredMobilityReservationRulesChecks
{
    public static void TieredMobilityReservationValidatorReportsEveryProblem()
    {
        var validator = TieredMobilityReservationRules.DefaultTieredMobilityReservationValidator();
        var item = TieredMobilityReservation.NewTieredMobilityReservation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TieredMobilityReservationMarginRangeRuleFlagsValuesAboveMargin()
    {
        var rule = new TieredMobilityReservationMarginRangeRule(4.5, 4886.0);
        var item = TieredMobilityReservation.NewTieredMobilityReservation("x", "y");
        item.Margin = 4887.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TieredMobilityReservationOwnerIdLengthRuleFlagsALongOwnerId()
    {
        var rule = new TieredMobilityReservationOwnerIdLengthRule(120);
        var item = TieredMobilityReservation.NewTieredMobilityReservation("x", "y");
        item.OwnerId = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.OwnerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TieredMobilityReservationRequiredOwnerIdRuleFlagsABlankOwnerId()
    {
        var rule = new TieredMobilityReservationRequiredOwnerIdRule("tiered mobility reservation needs this field");
        var item = TieredMobilityReservation.NewTieredMobilityReservation("", "");
        item.OwnerId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        TieredMobilityReservationValidatorReportsEveryProblem();
        TieredMobilityReservationMarginRangeRuleFlagsValuesAboveMargin();
        TieredMobilityReservationOwnerIdLengthRuleFlagsALongOwnerId();
        TieredMobilityReservationRequiredOwnerIdRuleFlagsABlankOwnerId();
    }
}
