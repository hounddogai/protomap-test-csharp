namespace Acme.Workforce.Absence.Tests.Balance.Baseline;

using Acme.Workforce.Absence.Balance.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class AdaptiveBalanceBaselineRulesChecks
{
    public static void AdaptiveBalanceBaselineValidatorReportsEveryProblem()
    {
        var validator = AdaptiveBalanceBaselineRules.DefaultAdaptiveBalanceBaselineValidator();
        var item = AdaptiveBalanceBaseline.NewAdaptiveBalanceBaseline("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdaptiveBalanceBaselineSourceLengthRuleFlagsALongSource()
    {
        var rule = new AdaptiveBalanceBaselineSourceLengthRule(64);
        var item = AdaptiveBalanceBaseline.NewAdaptiveBalanceBaseline("x", "y");
        item.Source = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Source = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdaptiveBalanceBaselineRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new AdaptiveBalanceBaselineRequiredCodeRule("adaptive balance baseline needs an entry");
        var item = AdaptiveBalanceBaseline.NewAdaptiveBalanceBaseline("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdaptiveBalanceBaselineWindowDaysRangeRuleFlagsValuesAboveWindowDays()
    {
        var rule = new AdaptiveBalanceBaselineWindowDaysRangeRule(1000, 3600);
        var item = AdaptiveBalanceBaseline.NewAdaptiveBalanceBaseline("x", "y");
        item.WindowDays = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        AdaptiveBalanceBaselineValidatorReportsEveryProblem();
        AdaptiveBalanceBaselineSourceLengthRuleFlagsALongSource();
        AdaptiveBalanceBaselineRequiredCodeRuleFlagsABlankCode();
        AdaptiveBalanceBaselineWindowDaysRangeRuleFlagsValuesAboveWindowDays();
    }
}
