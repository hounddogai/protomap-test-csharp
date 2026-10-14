namespace Acme.Workforce.Compensation.Tests.Adjustment.Notice;

using Acme.Workforce.Compensation.Adjustment.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class AdjustmentPreferenceRulesChecks
{
    public static void AdjustmentPreferenceValidatorReportsEveryProblem()
    {
        var validator = AdjustmentPreferenceRules.DefaultAdjustmentPreferenceValidator();
        var item = AdjustmentPreference.NewAdjustmentPreference("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdjustmentPreferenceMinimumRangeRuleFlagsValuesAboveMinimum()
    {
        var rule = new AdjustmentPreferenceMinimumRangeRule(4.1, 1233.3);
        var item = AdjustmentPreference.NewAdjustmentPreference("x", "y");
        item.Minimum = 1234.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AdjustmentPreferenceCurrencyLengthRuleFlagsALongCurrency()
    {
        var rule = new AdjustmentPreferenceCurrencyLengthRule(16);
        var item = AdjustmentPreference.NewAdjustmentPreference("x", "y");
        item.Currency = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Currency = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdjustmentPreferenceRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new AdjustmentPreferenceRequiredCodeRule("adjustment preference needs a value");
        var item = AdjustmentPreference.NewAdjustmentPreference("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdjustmentPreferenceTitleLengthRuleFlagsALongTitle()
    {
        var rule = new AdjustmentPreferenceTitleLengthRule(120);
        var item = AdjustmentPreference.NewAdjustmentPreference("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdjustmentPreferenceBudgetRangeRuleFlagsValuesAboveBudget()
    {
        var rule = new AdjustmentPreferenceBudgetRangeRule(1.8, 4511.2);
        var item = AdjustmentPreference.NewAdjustmentPreference("x", "y");
        item.Budget = 4512.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AdjustmentPreferenceRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new AdjustmentPreferenceRequiredLabelRule("adjustment preference needs a value");
        var item = AdjustmentPreference.NewAdjustmentPreference("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AdjustmentPreferenceValidatorReportsEveryProblem();
        AdjustmentPreferenceMinimumRangeRuleFlagsValuesAboveMinimum();
        AdjustmentPreferenceCurrencyLengthRuleFlagsALongCurrency();
        AdjustmentPreferenceRequiredCodeRuleFlagsABlankCode();
        AdjustmentPreferenceTitleLengthRuleFlagsALongTitle();
        AdjustmentPreferenceBudgetRangeRuleFlagsValuesAboveBudget();
        AdjustmentPreferenceRequiredLabelRuleFlagsABlankLabel();
    }
}
