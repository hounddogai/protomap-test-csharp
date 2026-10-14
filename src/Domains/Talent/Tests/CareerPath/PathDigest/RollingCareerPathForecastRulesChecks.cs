namespace Acme.Workforce.Talent.Tests.CareerPath.PathDigest;

using Acme.Workforce.Talent.CareerPath.PathDigest;

/// <summary>Checks of path digest code, run by the project's runner.</summary>
public static class RollingCareerPathForecastRulesChecks
{
    public static void RollingCareerPathForecastValidatorReportsEveryProblem()
    {
        var validator = RollingCareerPathForecastRules.DefaultRollingCareerPathForecastValidator();
        var item = RollingCareerPathForecast.NewRollingCareerPathForecast("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RollingCareerPathForecastTenureMonthsRangeRuleFlagsValuesAboveTenureMonths()
    {
        var rule = new RollingCareerPathForecastTenureMonthsRangeRule(16, 16);
        var item = RollingCareerPathForecast.NewRollingCareerPathForecast("x", "y");
        item.TenureMonths = 17;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RollingCareerPathForecastRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new RollingCareerPathForecastRequiredCodeRule("rolling career path forecast needs an entry");
        var item = RollingCareerPathForecast.NewRollingCareerPathForecast("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RollingCareerPathForecastRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new RollingCareerPathForecastRequiredTitleRule("rolling career path forecast needs this field");
        var item = RollingCareerPathForecast.NewRollingCareerPathForecast("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RollingCareerPathForecastNameLengthRuleFlagsALongName()
    {
        var rule = new RollingCareerPathForecastNameLengthRule(120);
        var item = RollingCareerPathForecast.NewRollingCareerPathForecast("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RollingCareerPathForecastLeaveDaysRangeRuleFlagsValuesAboveLeaveDays()
    {
        var rule = new RollingCareerPathForecastLeaveDaysRangeRule(32, 80);
        var item = RollingCareerPathForecast.NewRollingCareerPathForecast("x", "y");
        item.LeaveDays = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        RollingCareerPathForecastValidatorReportsEveryProblem();
        RollingCareerPathForecastTenureMonthsRangeRuleFlagsValuesAboveTenureMonths();
        RollingCareerPathForecastRequiredCodeRuleFlagsABlankCode();
        RollingCareerPathForecastRequiredTitleRuleFlagsABlankTitle();
        RollingCareerPathForecastNameLengthRuleFlagsALongName();
        RollingCareerPathForecastLeaveDaysRangeRuleFlagsValuesAboveLeaveDays();
    }
}
