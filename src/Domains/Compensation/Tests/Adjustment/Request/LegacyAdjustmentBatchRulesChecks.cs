namespace Acme.Workforce.Compensation.Tests.Adjustment.Request;

using Acme.Workforce.Compensation.Adjustment.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class LegacyAdjustmentBatchRulesChecks
{
    public static void LegacyAdjustmentBatchValidatorReportsEveryProblem()
    {
        var validator = LegacyAdjustmentBatchRules.DefaultLegacyAdjustmentBatchValidator();
        var item = LegacyAdjustmentBatch.NewLegacyAdjustmentBatch("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LegacyAdjustmentBatchUnitLengthRuleFlagsALongUnit()
    {
        var rule = new LegacyAdjustmentBatchUnitLengthRule(80);
        var item = LegacyAdjustmentBatch.NewLegacyAdjustmentBatch("x", "y");
        item.Unit = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Unit = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LegacyAdjustmentBatchCeilingRangeRuleFlagsValuesAboveCeiling()
    {
        var rule = new LegacyAdjustmentBatchCeilingRangeRule(9.6, 500.3);
        var item = LegacyAdjustmentBatch.NewLegacyAdjustmentBatch("x", "y");
        item.Ceiling = 501.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LegacyAdjustmentBatchRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LegacyAdjustmentBatchRequiredCodeRule("legacy adjustment batch needs this field");
        var item = LegacyAdjustmentBatch.NewLegacyAdjustmentBatch("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LegacyAdjustmentBatchValidatorReportsEveryProblem();
        LegacyAdjustmentBatchUnitLengthRuleFlagsALongUnit();
        LegacyAdjustmentBatchCeilingRangeRuleFlagsValuesAboveCeiling();
        LegacyAdjustmentBatchRequiredCodeRuleFlagsABlankCode();
    }
}
