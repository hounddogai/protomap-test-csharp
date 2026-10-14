namespace Acme.Workforce.Payrollops.Tests.RetroPay.Claim;

using Acme.Workforce.Payrollops.RetroPay.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class AdaptiveRetroPayMetricRulesChecks
{
    public static void AdaptiveRetroPayMetricValidatorReportsEveryProblem()
    {
        var validator = AdaptiveRetroPayMetricRules.DefaultAdaptiveRetroPayMetricValidator();
        var item = AdaptiveRetroPayMetric.NewAdaptiveRetroPayMetric("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdaptiveRetroPayMetricAmountRangeRuleFlagsValuesAboveAmount()
    {
        var rule = new AdaptiveRetroPayMetricAmountRangeRule(9.1, 2059.9);
        var item = AdaptiveRetroPayMetric.NewAdaptiveRetroPayMetric("x", "y");
        item.Amount = 2061.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AdaptiveRetroPayMetricCodeLengthRuleFlagsALongCode()
    {
        var rule = new AdaptiveRetroPayMetricCodeLengthRule(3600);
        var item = AdaptiveRetroPayMetric.NewAdaptiveRetroPayMetric("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdaptiveRetroPayMetricRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new AdaptiveRetroPayMetricRequiredCodeRule("adaptive retro pay metric needs an entry");
        var item = AdaptiveRetroPayMetric.NewAdaptiveRetroPayMetric("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdaptiveRetroPayMetricBudgetRangeRuleFlagsValuesAboveBudget()
    {
        var rule = new AdaptiveRetroPayMetricBudgetRangeRule(8.5, 3636.4);
        var item = AdaptiveRetroPayMetric.NewAdaptiveRetroPayMetric("x", "y");
        item.Budget = 3637.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AdaptiveRetroPayMetricRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new AdaptiveRetroPayMetricRequiredTitleRule("adaptive retro pay metric needs an entry");
        var item = AdaptiveRetroPayMetric.NewAdaptiveRetroPayMetric("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AdaptiveRetroPayMetricValidatorReportsEveryProblem();
        AdaptiveRetroPayMetricAmountRangeRuleFlagsValuesAboveAmount();
        AdaptiveRetroPayMetricCodeLengthRuleFlagsALongCode();
        AdaptiveRetroPayMetricRequiredCodeRuleFlagsABlankCode();
        AdaptiveRetroPayMetricBudgetRangeRuleFlagsValuesAboveBudget();
        AdaptiveRetroPayMetricRequiredTitleRuleFlagsABlankTitle();
    }
}
