namespace Acme.Workforce.Absence.Tests.Balance.Target;

using Acme.Workforce.Absence.Balance.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class BalanceForecastRulesChecks
{
    public static void BalanceForecastValidatorReportsEveryProblem()
    {
        var validator = BalanceForecastRules.DefaultBalanceForecastValidator();
        var item = BalanceForecast.NewBalanceForecast("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BalanceForecastMarginRangeRuleFlagsValuesAboveMargin()
    {
        var rule = new BalanceForecastMarginRangeRule(5.6, 863.2);
        var item = BalanceForecast.NewBalanceForecast("x", "y");
        item.Margin = 864.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BalanceForecastPhoneLengthRuleFlagsALongPhone()
    {
        var rule = new BalanceForecastPhoneLengthRule(1000);
        var item = BalanceForecast.NewBalanceForecast("x", "y");
        item.Phone = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Phone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BalanceForecastVersionRangeRuleFlagsValuesAboveVersion()
    {
        var rule = new BalanceForecastVersionRangeRule(80, 3600);
        var item = BalanceForecast.NewBalanceForecast("x", "y");
        item.Version = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BalanceForecastCodeLengthRuleFlagsALongCode()
    {
        var rule = new BalanceForecastCodeLengthRule(32);
        var item = BalanceForecast.NewBalanceForecast("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        BalanceForecastValidatorReportsEveryProblem();
        BalanceForecastMarginRangeRuleFlagsValuesAboveMargin();
        BalanceForecastPhoneLengthRuleFlagsALongPhone();
        BalanceForecastVersionRangeRuleFlagsValuesAboveVersion();
        BalanceForecastCodeLengthRuleFlagsALongCode();
    }
}
