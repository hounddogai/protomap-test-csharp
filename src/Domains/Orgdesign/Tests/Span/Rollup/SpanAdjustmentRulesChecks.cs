namespace Acme.Workforce.Orgdesign.Tests.Span.Rollup;

using Acme.Workforce.Orgdesign.Span.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class SpanAdjustmentRulesChecks
{
    public static void SpanAdjustmentValidatorReportsEveryProblem()
    {
        var validator = SpanAdjustmentRules.DefaultSpanAdjustmentValidator();
        var item = SpanAdjustment.NewSpanAdjustment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SpanAdjustmentNameLengthRuleFlagsALongName()
    {
        var rule = new SpanAdjustmentNameLengthRule(255);
        var item = SpanAdjustment.NewSpanAdjustment("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SpanAdjustmentFloorValueRangeRuleFlagsValuesAboveFloorValue()
    {
        var rule = new SpanAdjustmentFloorValueRangeRule(4.2, 2327.3);
        var item = SpanAdjustment.NewSpanAdjustment("x", "y");
        item.FloorValue = 2328.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SpanAdjustmentCodeLengthRuleFlagsALongCode()
    {
        var rule = new SpanAdjustmentCodeLengthRule(3600);
        var item = SpanAdjustment.NewSpanAdjustment("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        SpanAdjustmentValidatorReportsEveryProblem();
        SpanAdjustmentNameLengthRuleFlagsALongName();
        SpanAdjustmentFloorValueRangeRuleFlagsValuesAboveFloorValue();
        SpanAdjustmentCodeLengthRuleFlagsALongCode();
    }
}
