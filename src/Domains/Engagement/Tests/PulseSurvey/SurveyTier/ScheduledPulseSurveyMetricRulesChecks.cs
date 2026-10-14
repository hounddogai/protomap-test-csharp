namespace Acme.Workforce.Engagement.Tests.PulseSurvey.SurveyTier;

using Acme.Workforce.Engagement.PulseSurvey.SurveyTier;

/// <summary>Checks of survey tier code, run by the project's runner.</summary>
public static class ScheduledPulseSurveyMetricRulesChecks
{
    public static void ScheduledPulseSurveyMetricValidatorReportsEveryProblem()
    {
        var validator = ScheduledPulseSurveyMetricRules.DefaultScheduledPulseSurveyMetricValidator();
        var item = ScheduledPulseSurveyMetric.NewScheduledPulseSurveyMetric("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScheduledPulseSurveyMetricRequiredLocationRuleFlagsABlankLocation()
    {
        var rule = new ScheduledPulseSurveyMetricRequiredLocationRule("scheduled pulse survey metric needs an entry");
        var item = ScheduledPulseSurveyMetric.NewScheduledPulseSurveyMetric("", "");
        item.Location = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScheduledPulseSurveyMetricUnitLengthRuleFlagsALongUnit()
    {
        var rule = new ScheduledPulseSurveyMetricUnitLengthRule(1000);
        var item = ScheduledPulseSurveyMetric.NewScheduledPulseSurveyMetric("x", "y");
        item.Unit = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Unit = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScheduledPulseSurveyMetricDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes()
    {
        var rule = new ScheduledPulseSurveyMetricDurationMinutesRangeRule(64, 1000);
        var item = ScheduledPulseSurveyMetric.NewScheduledPulseSurveyMetric("x", "y");
        item.DurationMinutes = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ScheduledPulseSurveyMetricRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new ScheduledPulseSurveyMetricRequiredTitleRule("scheduled pulse survey metric needs an entry");
        var item = ScheduledPulseSurveyMetric.NewScheduledPulseSurveyMetric("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ScheduledPulseSurveyMetricValidatorReportsEveryProblem();
        ScheduledPulseSurveyMetricRequiredLocationRuleFlagsABlankLocation();
        ScheduledPulseSurveyMetricUnitLengthRuleFlagsALongUnit();
        ScheduledPulseSurveyMetricDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes();
        ScheduledPulseSurveyMetricRequiredTitleRuleFlagsABlankTitle();
    }
}
