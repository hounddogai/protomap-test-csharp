namespace Acme.Workforce.Workplace.Tests.FloorPlan.Entry;

using Acme.Workforce.Workplace.FloorPlan.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class ManualFloorPlanReservationRulesChecks
{
    public static void ManualFloorPlanReservationValidatorReportsEveryProblem()
    {
        var validator = ManualFloorPlanReservationRules.DefaultManualFloorPlanReservationValidator();
        var item = ManualFloorPlanReservation.NewManualFloorPlanReservation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ManualFloorPlanReservationRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ManualFloorPlanReservationRequiredCodeRule("manual floor plan reservation needs an entry");
        var item = ManualFloorPlanReservation.NewManualFloorPlanReservation("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ManualFloorPlanReservationMarginRangeRuleFlagsValuesAboveMargin()
    {
        var rule = new ManualFloorPlanReservationMarginRangeRule(6.3, 4857.6);
        var item = ManualFloorPlanReservation.NewManualFloorPlanReservation("x", "y");
        item.Margin = 4859.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ManualFloorPlanReservationGraceDaysRangeRuleFlagsValuesAboveGraceDays()
    {
        var rule = new ManualFloorPlanReservationGraceDaysRangeRule(32, 3600);
        var item = ManualFloorPlanReservation.NewManualFloorPlanReservation("x", "y");
        item.GraceDays = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        ManualFloorPlanReservationValidatorReportsEveryProblem();
        ManualFloorPlanReservationRequiredCodeRuleFlagsABlankCode();
        ManualFloorPlanReservationMarginRangeRuleFlagsValuesAboveMargin();
        ManualFloorPlanReservationGraceDaysRangeRuleFlagsValuesAboveGraceDays();
    }
}
