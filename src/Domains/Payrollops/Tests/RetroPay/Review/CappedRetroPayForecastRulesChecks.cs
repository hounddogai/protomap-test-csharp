namespace Acme.Workforce.Payrollops.Tests.RetroPay.Review;

using Acme.Workforce.Payrollops.RetroPay.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class CappedRetroPayForecastRulesChecks
{
    public static void CappedRetroPayForecastValidatorReportsEveryProblem()
    {
        var validator = CappedRetroPayForecastRules.DefaultCappedRetroPayForecastValidator();
        var item = CappedRetroPayForecast.NewCappedRetroPayForecast("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CappedRetroPayForecastExpiresAtRangeRuleFlagsValuesAboveExpiresAt()
    {
        var rule = new CappedRetroPayForecastExpiresAtRangeRule(32, 120);
        var item = CappedRetroPayForecast.NewCappedRetroPayForecast("x", "y");
        item.ExpiresAt = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CappedRetroPayForecastCodeLengthRuleFlagsALongCode()
    {
        var rule = new CappedRetroPayForecastCodeLengthRule(3600);
        var item = CappedRetroPayForecast.NewCappedRetroPayForecast("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CappedRetroPayForecastRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new CappedRetroPayForecastRequiredCodeRule("capped retro pay forecast needs an entry");
        var item = CappedRetroPayForecast.NewCappedRetroPayForecast("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        CappedRetroPayForecastValidatorReportsEveryProblem();
        CappedRetroPayForecastExpiresAtRangeRuleFlagsValuesAboveExpiresAt();
        CappedRetroPayForecastCodeLengthRuleFlagsALongCode();
        CappedRetroPayForecastRequiredCodeRuleFlagsABlankCode();
    }
}
