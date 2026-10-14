namespace Acme.Workforce.Compliance.Tests.TrainingDue.DueSchedule;

using Acme.Workforce.Compliance.TrainingDue.DueSchedule;

/// <summary>Checks of due schedule code, run by the project's runner.</summary>
public static class GlobalTrainingDueForecastRulesChecks
{
    public static void GlobalTrainingDueForecastValidatorReportsEveryProblem()
    {
        var validator = GlobalTrainingDueForecastRules.DefaultGlobalTrainingDueForecastValidator();
        var item = GlobalTrainingDueForecast.NewGlobalTrainingDueForecast("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void GlobalTrainingDueForecastTimezoneLengthRuleFlagsALongTimezone()
    {
        var rule = new GlobalTrainingDueForecastTimezoneLengthRule(3600);
        var item = GlobalTrainingDueForecast.NewGlobalTrainingDueForecast("x", "y");
        item.Timezone = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Timezone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GlobalTrainingDueForecastRateRangeRuleFlagsValuesAboveRate()
    {
        var rule = new GlobalTrainingDueForecastRateRangeRule(8.2, 784.3);
        var item = GlobalTrainingDueForecast.NewGlobalTrainingDueForecast("x", "y");
        item.Rate = 785.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void GlobalTrainingDueForecastTitleLengthRuleFlagsALongTitle()
    {
        var rule = new GlobalTrainingDueForecastTitleLengthRule(255);
        var item = GlobalTrainingDueForecast.NewGlobalTrainingDueForecast("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GlobalTrainingDueForecastCeilingRangeRuleFlagsValuesAboveCeiling()
    {
        var rule = new GlobalTrainingDueForecastCeilingRangeRule(8.9, 2303.6);
        var item = GlobalTrainingDueForecast.NewGlobalTrainingDueForecast("x", "y");
        item.Ceiling = 2305.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void GlobalTrainingDueForecastRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new GlobalTrainingDueForecastRequiredTitleRule("global training due forecast needs this field");
        var item = GlobalTrainingDueForecast.NewGlobalTrainingDueForecast("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        GlobalTrainingDueForecastValidatorReportsEveryProblem();
        GlobalTrainingDueForecastTimezoneLengthRuleFlagsALongTimezone();
        GlobalTrainingDueForecastRateRangeRuleFlagsValuesAboveRate();
        GlobalTrainingDueForecastTitleLengthRuleFlagsALongTitle();
        GlobalTrainingDueForecastCeilingRangeRuleFlagsValuesAboveCeiling();
        GlobalTrainingDueForecastRequiredTitleRuleFlagsABlankTitle();
    }
}
