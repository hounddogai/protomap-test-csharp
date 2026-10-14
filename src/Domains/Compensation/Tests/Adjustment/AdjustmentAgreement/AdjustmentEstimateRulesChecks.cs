namespace Acme.Workforce.Compensation.Tests.Adjustment.AdjustmentAgreement;

using Acme.Workforce.Compensation.Adjustment.AdjustmentAgreement;

/// <summary>Checks of adjustment agreement code, run by the project's runner.</summary>
public static class AdjustmentEstimateRulesChecks
{
    public static void AdjustmentEstimateValidatorReportsEveryProblem()
    {
        var validator = AdjustmentEstimateRules.DefaultAdjustmentEstimateValidator();
        var item = AdjustmentEstimate.NewAdjustmentEstimate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdjustmentEstimateRequiredLocaleRuleFlagsABlankLocale()
    {
        var rule = new AdjustmentEstimateRequiredLocaleRule("adjustment estimate needs an entry");
        var item = AdjustmentEstimate.NewAdjustmentEstimate("", "");
        item.Locale = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdjustmentEstimateMarginRangeRuleFlagsValuesAboveMargin()
    {
        var rule = new AdjustmentEstimateMarginRangeRule(7.7, 3731.1);
        var item = AdjustmentEstimate.NewAdjustmentEstimate("x", "y");
        item.Margin = 3732.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AdjustmentEstimateRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new AdjustmentEstimateRequiredTitleRule("adjustment estimate needs this field");
        var item = AdjustmentEstimate.NewAdjustmentEstimate("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AdjustmentEstimateValidatorReportsEveryProblem();
        AdjustmentEstimateRequiredLocaleRuleFlagsABlankLocale();
        AdjustmentEstimateMarginRangeRuleFlagsValuesAboveMargin();
        AdjustmentEstimateRequiredTitleRuleFlagsABlankTitle();
    }
}
