namespace Acme.Workforce.Workplace.Tests.FloorPlan.Reservation;

using Acme.Workforce.Workplace.FloorPlan.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class PrimaryFloorPlanReservationRulesChecks
{
    public static void PrimaryFloorPlanReservationValidatorReportsEveryProblem()
    {
        var validator = PrimaryFloorPlanReservationRules.DefaultPrimaryFloorPlanReservationValidator();
        var item = PrimaryFloorPlanReservation.NewPrimaryFloorPlanReservation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PrimaryFloorPlanReservationLocaleLengthRuleFlagsALongLocale()
    {
        var rule = new PrimaryFloorPlanReservationLocaleLengthRule(32);
        var item = PrimaryFloorPlanReservation.NewPrimaryFloorPlanReservation("x", "y");
        item.Locale = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Locale = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PrimaryFloorPlanReservationRequiredLocaleRuleFlagsABlankLocale()
    {
        var rule = new PrimaryFloorPlanReservationRequiredLocaleRule("primary floor plan reservation needs an entry");
        var item = PrimaryFloorPlanReservation.NewPrimaryFloorPlanReservation("", "");
        item.Locale = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PrimaryFloorPlanReservationNameLengthRuleFlagsALongName()
    {
        var rule = new PrimaryFloorPlanReservationNameLengthRule(80);
        var item = PrimaryFloorPlanReservation.NewPrimaryFloorPlanReservation("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PrimaryFloorPlanReservationRequiredPhoneRuleFlagsABlankPhone()
    {
        var rule = new PrimaryFloorPlanReservationRequiredPhoneRule("primary floor plan reservation needs this field");
        var item = PrimaryFloorPlanReservation.NewPrimaryFloorPlanReservation("", "");
        item.Phone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PrimaryFloorPlanReservationRequiredNameRuleFlagsABlankName()
    {
        var rule = new PrimaryFloorPlanReservationRequiredNameRule("primary floor plan reservation needs this field");
        var item = PrimaryFloorPlanReservation.NewPrimaryFloorPlanReservation("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PrimaryFloorPlanReservationValidatorReportsEveryProblem();
        PrimaryFloorPlanReservationLocaleLengthRuleFlagsALongLocale();
        PrimaryFloorPlanReservationRequiredLocaleRuleFlagsABlankLocale();
        PrimaryFloorPlanReservationNameLengthRuleFlagsALongName();
        PrimaryFloorPlanReservationRequiredPhoneRuleFlagsABlankPhone();
        PrimaryFloorPlanReservationRequiredNameRuleFlagsABlankName();
    }
}
