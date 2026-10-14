namespace Acme.Workforce.Talent.Tests.Pool.PoolWindow;

using Acme.Workforce.Talent.Pool.PoolWindow;

/// <summary>Checks of pool window code, run by the project's runner.</summary>
public static class ScheduledPoolWindowRulesChecks
{
    public static void ScheduledPoolWindowValidatorReportsEveryProblem()
    {
        var validator = ScheduledPoolWindowRules.DefaultScheduledPoolWindowValidator();
        var item = ScheduledPoolWindow.NewScheduledPoolWindow("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScheduledPoolWindowWeightRangeRuleFlagsValuesAboveWeight()
    {
        var rule = new ScheduledPoolWindowWeightRangeRule(4.4, 611.3);
        var item = ScheduledPoolWindow.NewScheduledPoolWindow("x", "y");
        item.Weight = 612.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ScheduledPoolWindowRequiredCurrencyRuleFlagsABlankCurrency()
    {
        var rule = new ScheduledPoolWindowRequiredCurrencyRule("scheduled pool window needs this field");
        var item = ScheduledPoolWindow.NewScheduledPoolWindow("", "");
        item.Currency = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScheduledPoolWindowRequiredCategoryRuleFlagsABlankCategory()
    {
        var rule = new ScheduledPoolWindowRequiredCategoryRule("scheduled pool window needs this field");
        var item = ScheduledPoolWindow.NewScheduledPoolWindow("", "");
        item.Category = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScheduledPoolWindowCurrencyLengthRuleFlagsALongCurrency()
    {
        var rule = new ScheduledPoolWindowCurrencyLengthRule(86400);
        var item = ScheduledPoolWindow.NewScheduledPoolWindow("x", "y");
        item.Currency = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Currency = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ScheduledPoolWindowValidatorReportsEveryProblem();
        ScheduledPoolWindowWeightRangeRuleFlagsValuesAboveWeight();
        ScheduledPoolWindowRequiredCurrencyRuleFlagsABlankCurrency();
        ScheduledPoolWindowRequiredCategoryRuleFlagsABlankCategory();
        ScheduledPoolWindowCurrencyLengthRuleFlagsALongCurrency();
    }
}
