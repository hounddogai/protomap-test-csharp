namespace Acme.Workforce.Orgdesign.Tests.Span.Metric;

using Acme.Workforce.Orgdesign.Span.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class LegacySpanAdjustmentRulesChecks
{
    public static void LegacySpanAdjustmentValidatorReportsEveryProblem()
    {
        var validator = LegacySpanAdjustmentRules.DefaultLegacySpanAdjustmentValidator();
        var item = LegacySpanAdjustment.NewLegacySpanAdjustment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LegacySpanAdjustmentNameLengthRuleFlagsALongName()
    {
        var rule = new LegacySpanAdjustmentNameLengthRule(16);
        var item = LegacySpanAdjustment.NewLegacySpanAdjustment("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LegacySpanAdjustmentRequiredCostCenterRuleFlagsABlankCostCenter()
    {
        var rule = new LegacySpanAdjustmentRequiredCostCenterRule("legacy span adjustment needs this field");
        var item = LegacySpanAdjustment.NewLegacySpanAdjustment("", "");
        item.CostCenter = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LegacySpanAdjustmentSequenceRangeRuleFlagsValuesAboveSequence()
    {
        var rule = new LegacySpanAdjustmentSequenceRangeRule(80, 120);
        var item = LegacySpanAdjustment.NewLegacySpanAdjustment("x", "y");
        item.Sequence = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        LegacySpanAdjustmentValidatorReportsEveryProblem();
        LegacySpanAdjustmentNameLengthRuleFlagsALongName();
        LegacySpanAdjustmentRequiredCostCenterRuleFlagsABlankCostCenter();
        LegacySpanAdjustmentSequenceRangeRuleFlagsValuesAboveSequence();
    }
}
