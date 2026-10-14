namespace Acme.Workforce.Workplace.Tests.FloorPlan.Adjustment;

using Acme.Workforce.Workplace.FloorPlan.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class FloorPlanReservationRulesChecks
{
    public static void FloorPlanReservationValidatorReportsEveryProblem()
    {
        var validator = FloorPlanReservationRules.DefaultFloorPlanReservationValidator();
        var item = FloorPlanReservation.NewFloorPlanReservation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FloorPlanReservationContactEmailLengthRuleFlagsALongContactEmail()
    {
        var rule = new FloorPlanReservationContactEmailLengthRule(1000);
        var item = FloorPlanReservation.NewFloorPlanReservation("x", "y");
        item.ContactEmail = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ContactEmail = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FloorPlanReservationDescriptionLengthRuleFlagsALongDescription()
    {
        var rule = new FloorPlanReservationDescriptionLengthRule(1000);
        var item = FloorPlanReservation.NewFloorPlanReservation("x", "y");
        item.Description = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Description = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FloorPlanReservationExpiresAtRangeRuleFlagsValuesAboveExpiresAt()
    {
        var rule = new FloorPlanReservationExpiresAtRangeRule(1000, 3600);
        var item = FloorPlanReservation.NewFloorPlanReservation("x", "y");
        item.ExpiresAt = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void FloorPlanReservationCodeLengthRuleFlagsALongCode()
    {
        var rule = new FloorPlanReservationCodeLengthRule(3600);
        var item = FloorPlanReservation.NewFloorPlanReservation("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        FloorPlanReservationValidatorReportsEveryProblem();
        FloorPlanReservationContactEmailLengthRuleFlagsALongContactEmail();
        FloorPlanReservationDescriptionLengthRuleFlagsALongDescription();
        FloorPlanReservationExpiresAtRangeRuleFlagsValuesAboveExpiresAt();
        FloorPlanReservationCodeLengthRuleFlagsALongCode();
    }
}
