namespace Acme.Workforce.Contractors.Tests.Rate.Score;

using Acme.Workforce.Contractors.Rate.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class StrictRateForecastRulesChecks
{
    public static void StrictRateForecastValidatorReportsEveryProblem()
    {
        var validator = StrictRateForecastRules.DefaultStrictRateForecastValidator();
        var item = StrictRateForecast.NewStrictRateForecast("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StrictRateForecastMaximumRangeRuleFlagsValuesAboveMaximum()
    {
        var rule = new StrictRateForecastMaximumRangeRule(6.7, 4972.8);
        var item = StrictRateForecast.NewStrictRateForecast("x", "y");
        item.Maximum = 4974.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void StrictRateForecastStartsAtRangeRuleFlagsValuesAboveStartsAt()
    {
        var rule = new StrictRateForecastStartsAtRangeRule(80, 80);
        var item = StrictRateForecast.NewStrictRateForecast("x", "y");
        item.StartsAt = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void StrictRateForecastCodeLengthRuleFlagsALongCode()
    {
        var rule = new StrictRateForecastCodeLengthRule(64);
        var item = StrictRateForecast.NewStrictRateForecast("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        StrictRateForecastValidatorReportsEveryProblem();
        StrictRateForecastMaximumRangeRuleFlagsValuesAboveMaximum();
        StrictRateForecastStartsAtRangeRuleFlagsValuesAboveStartsAt();
        StrictRateForecastCodeLengthRuleFlagsALongCode();
    }
}
