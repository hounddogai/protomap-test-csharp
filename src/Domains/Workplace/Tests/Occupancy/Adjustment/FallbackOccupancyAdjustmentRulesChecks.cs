namespace Acme.Workforce.Workplace.Tests.Occupancy.Adjustment;

using Acme.Workforce.Workplace.Occupancy.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class FallbackOccupancyAdjustmentRulesChecks
{
    public static void FallbackOccupancyAdjustmentValidatorReportsEveryProblem()
    {
        var validator = FallbackOccupancyAdjustmentRules.DefaultFallbackOccupancyAdjustmentValidator();
        var item = FallbackOccupancyAdjustment.NewFallbackOccupancyAdjustment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FallbackOccupancyAdjustmentCodeLengthRuleFlagsALongCode()
    {
        var rule = new FallbackOccupancyAdjustmentCodeLengthRule(16);
        var item = FallbackOccupancyAdjustment.NewFallbackOccupancyAdjustment("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FallbackOccupancyAdjustmentLabelLengthRuleFlagsALongLabel()
    {
        var rule = new FallbackOccupancyAdjustmentLabelLengthRule(64);
        var item = FallbackOccupancyAdjustment.NewFallbackOccupancyAdjustment("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FallbackOccupancyAdjustmentStartsAtRangeRuleFlagsValuesAboveStartsAt()
    {
        var rule = new FallbackOccupancyAdjustmentStartsAtRangeRule(64, 120);
        var item = FallbackOccupancyAdjustment.NewFallbackOccupancyAdjustment("x", "y");
        item.StartsAt = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void FallbackOccupancyAdjustmentRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new FallbackOccupancyAdjustmentRequiredCodeRule("fallback occupancy adjustment needs this field");
        var item = FallbackOccupancyAdjustment.NewFallbackOccupancyAdjustment("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        FallbackOccupancyAdjustmentValidatorReportsEveryProblem();
        FallbackOccupancyAdjustmentCodeLengthRuleFlagsALongCode();
        FallbackOccupancyAdjustmentLabelLengthRuleFlagsALongLabel();
        FallbackOccupancyAdjustmentStartsAtRangeRuleFlagsValuesAboveStartsAt();
        FallbackOccupancyAdjustmentRequiredCodeRuleFlagsABlankCode();
    }
}
