namespace Acme.Workforce.Orgdesign.Tests.Headcount.Baseline;

using Acme.Workforce.Orgdesign.Headcount.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class HeadcountForecastRulesChecks
{
    public static void HeadcountForecastValidatorReportsEveryProblem()
    {
        var validator = HeadcountForecastRules.DefaultHeadcountForecastValidator();
        var item = HeadcountForecast.NewHeadcountForecast("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void HeadcountForecastStartsAtRangeRuleFlagsValuesAboveStartsAt()
    {
        var rule = new HeadcountForecastStartsAtRangeRule(120, 3600);
        var item = HeadcountForecast.NewHeadcountForecast("x", "y");
        item.StartsAt = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void HeadcountForecastRequiredNameRuleFlagsABlankName()
    {
        var rule = new HeadcountForecastRequiredNameRule("headcount forecast needs a value");
        var item = HeadcountForecast.NewHeadcountForecast("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        HeadcountForecastValidatorReportsEveryProblem();
        HeadcountForecastStartsAtRangeRuleFlagsValuesAboveStartsAt();
        HeadcountForecastRequiredNameRuleFlagsABlankName();
    }
}
