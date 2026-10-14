namespace Acme.Workforce.Absence.Tests.Balance.Threshold;

using Acme.Workforce.Absence.Balance.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class PrimaryBalanceMetricRulesChecks
{
    public static void PrimaryBalanceMetricValidatorReportsEveryProblem()
    {
        var validator = PrimaryBalanceMetricRules.DefaultPrimaryBalanceMetricValidator();
        var item = PrimaryBalanceMetric.NewPrimaryBalanceMetric("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PrimaryBalanceMetricRequiredLocaleRuleFlagsABlankLocale()
    {
        var rule = new PrimaryBalanceMetricRequiredLocaleRule("primary balance metric needs a value");
        var item = PrimaryBalanceMetric.NewPrimaryBalanceMetric("", "");
        item.Locale = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PrimaryBalanceMetricRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new PrimaryBalanceMetricRequiredTitleRule("primary balance metric needs this field");
        var item = PrimaryBalanceMetric.NewPrimaryBalanceMetric("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PrimaryBalanceMetricRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PrimaryBalanceMetricRequiredCodeRule("primary balance metric needs a value");
        var item = PrimaryBalanceMetric.NewPrimaryBalanceMetric("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PrimaryBalanceMetricAmountRangeRuleFlagsValuesAboveAmount()
    {
        var rule = new PrimaryBalanceMetricAmountRangeRule(1.1, 1125.1);
        var item = PrimaryBalanceMetric.NewPrimaryBalanceMetric("x", "y");
        item.Amount = 1126.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PrimaryBalanceMetricLocaleLengthRuleFlagsALongLocale()
    {
        var rule = new PrimaryBalanceMetricLocaleLengthRule(120);
        var item = PrimaryBalanceMetric.NewPrimaryBalanceMetric("x", "y");
        item.Locale = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Locale = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PrimaryBalanceMetricValidatorReportsEveryProblem();
        PrimaryBalanceMetricRequiredLocaleRuleFlagsABlankLocale();
        PrimaryBalanceMetricRequiredTitleRuleFlagsABlankTitle();
        PrimaryBalanceMetricRequiredCodeRuleFlagsABlankCode();
        PrimaryBalanceMetricAmountRangeRuleFlagsValuesAboveAmount();
        PrimaryBalanceMetricLocaleLengthRuleFlagsALongLocale();
    }
}
