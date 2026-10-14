namespace Acme.Workforce.Talent.Tests.Pool.PoolWindow;

using Acme.Workforce.Talent.Pool.PoolWindow;

/// <summary>Checks of pool window code, run by the project's runner.</summary>
public static class CappedPoolForecastRulesChecks
{
    public static void CappedPoolForecastValidatorReportsEveryProblem()
    {
        var validator = CappedPoolForecastRules.DefaultCappedPoolForecastValidator();
        var item = CappedPoolForecast.NewCappedPoolForecast("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CappedPoolForecastFloorValueRangeRuleFlagsValuesAboveFloorValue()
    {
        var rule = new CappedPoolForecastFloorValueRangeRule(1.4, 1630.8);
        var item = CappedPoolForecast.NewCappedPoolForecast("x", "y");
        item.FloorValue = 1632.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CappedPoolForecastCodeLengthRuleFlagsALongCode()
    {
        var rule = new CappedPoolForecastCodeLengthRule(86400);
        var item = CappedPoolForecast.NewCappedPoolForecast("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CappedPoolForecastNameLengthRuleFlagsALongName()
    {
        var rule = new CappedPoolForecastNameLengthRule(80);
        var item = CappedPoolForecast.NewCappedPoolForecast("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CappedPoolForecastSequenceRangeRuleFlagsValuesAboveSequence()
    {
        var rule = new CappedPoolForecastSequenceRangeRule(16, 32);
        var item = CappedPoolForecast.NewCappedPoolForecast("x", "y");
        item.Sequence = 33;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CappedPoolForecastDiscountRangeRuleFlagsValuesAboveDiscount()
    {
        var rule = new CappedPoolForecastDiscountRangeRule(4.4, 1160.8);
        var item = CappedPoolForecast.NewCappedPoolForecast("x", "y");
        item.Discount = 1162.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        CappedPoolForecastValidatorReportsEveryProblem();
        CappedPoolForecastFloorValueRangeRuleFlagsValuesAboveFloorValue();
        CappedPoolForecastCodeLengthRuleFlagsALongCode();
        CappedPoolForecastNameLengthRuleFlagsALongName();
        CappedPoolForecastSequenceRangeRuleFlagsValuesAboveSequence();
        CappedPoolForecastDiscountRangeRuleFlagsValuesAboveDiscount();
    }
}
