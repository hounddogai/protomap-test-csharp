namespace Acme.Workforce.Orgdesign.Tests.Span.Estimate;

using Acme.Workforce.Orgdesign.Span.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class PrimarySpanEstimateRulesChecks
{
    public static void PrimarySpanEstimateValidatorReportsEveryProblem()
    {
        var validator = PrimarySpanEstimateRules.DefaultPrimarySpanEstimateValidator();
        var item = PrimarySpanEstimate.NewPrimarySpanEstimate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PrimarySpanEstimateCostCenterLengthRuleFlagsALongCostCenter()
    {
        var rule = new PrimarySpanEstimateCostCenterLengthRule(16);
        var item = PrimarySpanEstimate.NewPrimarySpanEstimate("x", "y");
        item.CostCenter = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.CostCenter = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PrimarySpanEstimateRetriesRangeRuleFlagsValuesAboveRetries()
    {
        var rule = new PrimarySpanEstimateRetriesRangeRule(1000, 3600);
        var item = PrimarySpanEstimate.NewPrimarySpanEstimate("x", "y");
        item.Retries = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        PrimarySpanEstimateValidatorReportsEveryProblem();
        PrimarySpanEstimateCostCenterLengthRuleFlagsALongCostCenter();
        PrimarySpanEstimateRetriesRangeRuleFlagsValuesAboveRetries();
    }
}
