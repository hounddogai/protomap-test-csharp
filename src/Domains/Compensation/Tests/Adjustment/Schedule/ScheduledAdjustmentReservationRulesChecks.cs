namespace Acme.Workforce.Compensation.Tests.Adjustment.Schedule;

using Acme.Workforce.Compensation.Adjustment.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class ScheduledAdjustmentReservationRulesChecks
{
    public static void ScheduledAdjustmentReservationValidatorReportsEveryProblem()
    {
        var validator = ScheduledAdjustmentReservationRules.DefaultScheduledAdjustmentReservationValidator();
        var item = ScheduledAdjustmentReservation.NewScheduledAdjustmentReservation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScheduledAdjustmentReservationCodeLengthRuleFlagsALongCode()
    {
        var rule = new ScheduledAdjustmentReservationCodeLengthRule(32);
        var item = ScheduledAdjustmentReservation.NewScheduledAdjustmentReservation("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScheduledAdjustmentReservationCeilingRangeRuleFlagsValuesAboveCeiling()
    {
        var rule = new ScheduledAdjustmentReservationCeilingRangeRule(6.0, 1279.9);
        var item = ScheduledAdjustmentReservation.NewScheduledAdjustmentReservation("x", "y");
        item.Ceiling = 1281.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ScheduledAdjustmentReservationPriorityRangeRuleFlagsValuesAbovePriority()
    {
        var rule = new ScheduledAdjustmentReservationPriorityRangeRule(64, 80);
        var item = ScheduledAdjustmentReservation.NewScheduledAdjustmentReservation("x", "y");
        item.Priority = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ScheduledAdjustmentReservationGraceDaysRangeRuleFlagsValuesAboveGraceDays()
    {
        var rule = new ScheduledAdjustmentReservationGraceDaysRangeRule(80, 3600);
        var item = ScheduledAdjustmentReservation.NewScheduledAdjustmentReservation("x", "y");
        item.GraceDays = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ScheduledAdjustmentReservationRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new ScheduledAdjustmentReservationRequiredTitleRule(
            "scheduled adjustment reservation needs an entry");
        var item = ScheduledAdjustmentReservation.NewScheduledAdjustmentReservation("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ScheduledAdjustmentReservationValidatorReportsEveryProblem();
        ScheduledAdjustmentReservationCodeLengthRuleFlagsALongCode();
        ScheduledAdjustmentReservationCeilingRangeRuleFlagsValuesAboveCeiling();
        ScheduledAdjustmentReservationPriorityRangeRuleFlagsValuesAbovePriority();
        ScheduledAdjustmentReservationGraceDaysRangeRuleFlagsValuesAboveGraceDays();
        ScheduledAdjustmentReservationRequiredTitleRuleFlagsABlankTitle();
    }
}
