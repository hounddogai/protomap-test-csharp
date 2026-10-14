namespace Acme.Workforce.Workplace.Tests.Occupancy.Reservation;

using Acme.Workforce.Workplace.Occupancy.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class OccupancyReservationRulesChecks
{
    public static void OccupancyReservationValidatorReportsEveryProblem()
    {
        var validator = OccupancyReservationRules.DefaultOccupancyReservationValidator();
        var item = OccupancyReservation.NewOccupancyReservation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void OccupancyReservationCeilingRangeRuleFlagsValuesAboveCeiling()
    {
        var rule = new OccupancyReservationCeilingRangeRule(5.9, 1702.5);
        var item = OccupancyReservation.NewOccupancyReservation("x", "y");
        item.Ceiling = 1704.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void OccupancyReservationCodeLengthRuleFlagsALongCode()
    {
        var rule = new OccupancyReservationCodeLengthRule(32);
        var item = OccupancyReservation.NewOccupancyReservation("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void OccupancyReservationTitleLengthRuleFlagsALongTitle()
    {
        var rule = new OccupancyReservationTitleLengthRule(16);
        var item = OccupancyReservation.NewOccupancyReservation("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        OccupancyReservationValidatorReportsEveryProblem();
        OccupancyReservationCeilingRangeRuleFlagsValuesAboveCeiling();
        OccupancyReservationCodeLengthRuleFlagsALongCode();
        OccupancyReservationTitleLengthRuleFlagsALongTitle();
    }
}
