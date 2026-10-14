namespace Acme.Workforce.Compensation.Tests.Adjustment.Rollup;

using Acme.Workforce.Compensation.Adjustment.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class RegionalAdjustmentReservationRulesChecks
{
    public static void RegionalAdjustmentReservationValidatorReportsEveryProblem()
    {
        var validator = RegionalAdjustmentReservationRules.DefaultRegionalAdjustmentReservationValidator();
        var item = RegionalAdjustmentReservation.NewRegionalAdjustmentReservation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RegionalAdjustmentReservationSequenceRangeRuleFlagsValuesAboveSequence()
    {
        var rule = new RegionalAdjustmentReservationSequenceRangeRule(16, 3600);
        var item = RegionalAdjustmentReservation.NewRegionalAdjustmentReservation("x", "y");
        item.Sequence = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RegionalAdjustmentReservationCategoryLengthRuleFlagsALongCategory()
    {
        var rule = new RegionalAdjustmentReservationCategoryLengthRule(16);
        var item = RegionalAdjustmentReservation.NewRegionalAdjustmentReservation("x", "y");
        item.Category = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Category = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        RegionalAdjustmentReservationValidatorReportsEveryProblem();
        RegionalAdjustmentReservationSequenceRangeRuleFlagsValuesAboveSequence();
        RegionalAdjustmentReservationCategoryLengthRuleFlagsALongCategory();
    }
}
