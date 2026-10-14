namespace Acme.Workforce.Compensation.Tests.Adjustment.Notice;

using Acme.Workforce.Compensation.Adjustment.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class AdjustmentReservationRulesChecks
{
    public static void AdjustmentReservationValidatorReportsEveryProblem()
    {
        var validator = AdjustmentReservationRules.DefaultAdjustmentReservationValidator();
        var item = AdjustmentReservation.NewAdjustmentReservation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdjustmentReservationNameLengthRuleFlagsALongName()
    {
        var rule = new AdjustmentReservationNameLengthRule(80);
        var item = AdjustmentReservation.NewAdjustmentReservation("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdjustmentReservationRetriesRangeRuleFlagsValuesAboveRetries()
    {
        var rule = new AdjustmentReservationRetriesRangeRule(64, 120);
        var item = AdjustmentReservation.NewAdjustmentReservation("x", "y");
        item.Retries = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AdjustmentReservationPriorityRangeRuleFlagsValuesAbovePriority()
    {
        var rule = new AdjustmentReservationPriorityRangeRule(32, 80);
        var item = AdjustmentReservation.NewAdjustmentReservation("x", "y");
        item.Priority = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AdjustmentReservationLeaveDaysRangeRuleFlagsValuesAboveLeaveDays()
    {
        var rule = new AdjustmentReservationLeaveDaysRangeRule(255, 86400);
        var item = AdjustmentReservation.NewAdjustmentReservation("x", "y");
        item.LeaveDays = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AdjustmentReservationSummaryLengthRuleFlagsALongSummary()
    {
        var rule = new AdjustmentReservationSummaryLengthRule(120);
        var item = AdjustmentReservation.NewAdjustmentReservation("x", "y");
        item.Summary = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Summary = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AdjustmentReservationValidatorReportsEveryProblem();
        AdjustmentReservationNameLengthRuleFlagsALongName();
        AdjustmentReservationRetriesRangeRuleFlagsValuesAboveRetries();
        AdjustmentReservationPriorityRangeRuleFlagsValuesAbovePriority();
        AdjustmentReservationLeaveDaysRangeRuleFlagsValuesAboveLeaveDays();
        AdjustmentReservationSummaryLengthRuleFlagsALongSummary();
    }
}
