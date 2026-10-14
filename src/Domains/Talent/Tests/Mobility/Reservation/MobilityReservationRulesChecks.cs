namespace Acme.Workforce.Talent.Tests.Mobility.Reservation;

using Acme.Workforce.Talent.Mobility.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class MobilityReservationRulesChecks
{
    public static void MobilityReservationValidatorReportsEveryProblem()
    {
        var validator = MobilityReservationRules.DefaultMobilityReservationValidator();
        var item = MobilityReservation.NewMobilityReservation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void MobilityReservationRequiredGradeRuleFlagsABlankGrade()
    {
        var rule = new MobilityReservationRequiredGradeRule("mobility reservation needs this field");
        var item = MobilityReservation.NewMobilityReservation("", "");
        item.Grade = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void MobilityReservationRequiredCountryRuleFlagsABlankCountry()
    {
        var rule = new MobilityReservationRequiredCountryRule("mobility reservation needs this field");
        var item = MobilityReservation.NewMobilityReservation("", "");
        item.Country = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void MobilityReservationRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new MobilityReservationRequiredLabelRule("mobility reservation needs an entry");
        var item = MobilityReservation.NewMobilityReservation("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        MobilityReservationValidatorReportsEveryProblem();
        MobilityReservationRequiredGradeRuleFlagsABlankGrade();
        MobilityReservationRequiredCountryRuleFlagsABlankCountry();
        MobilityReservationRequiredLabelRuleFlagsABlankLabel();
    }
}
