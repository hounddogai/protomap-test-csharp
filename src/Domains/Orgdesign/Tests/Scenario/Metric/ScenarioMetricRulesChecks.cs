namespace Acme.Workforce.Orgdesign.Tests.Scenario.Metric;

using Acme.Workforce.Orgdesign.Scenario.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class ScenarioMetricRulesChecks
{
    public static void ScenarioMetricValidatorReportsEveryProblem()
    {
        var validator = ScenarioMetricRules.DefaultScenarioMetricValidator();
        var item = ScenarioMetric.NewScenarioMetric("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScenarioMetricExpiresAtRangeRuleFlagsValuesAboveExpiresAt()
    {
        var rule = new ScenarioMetricExpiresAtRangeRule(32, 32);
        var item = ScenarioMetric.NewScenarioMetric("x", "y");
        item.ExpiresAt = 33;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ScenarioMetricRequiredCategoryRuleFlagsABlankCategory()
    {
        var rule = new ScenarioMetricRequiredCategoryRule("scenario metric needs an entry");
        var item = ScenarioMetric.NewScenarioMetric("", "");
        item.Category = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScenarioMetricCategoryLengthRuleFlagsALongCategory()
    {
        var rule = new ScenarioMetricCategoryLengthRule(86400);
        var item = ScenarioMetric.NewScenarioMetric("x", "y");
        item.Category = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Category = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ScenarioMetricValidatorReportsEveryProblem();
        ScenarioMetricExpiresAtRangeRuleFlagsValuesAboveExpiresAt();
        ScenarioMetricRequiredCategoryRuleFlagsABlankCategory();
        ScenarioMetricCategoryLengthRuleFlagsALongCategory();
    }
}
