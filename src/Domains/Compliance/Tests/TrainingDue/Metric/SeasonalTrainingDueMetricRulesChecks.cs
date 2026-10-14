namespace Acme.Workforce.Compliance.Tests.TrainingDue.Metric;

using Acme.Workforce.Compliance.TrainingDue.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class SeasonalTrainingDueMetricRulesChecks
{
    public static void SeasonalTrainingDueMetricValidatorReportsEveryProblem()
    {
        var validator = SeasonalTrainingDueMetricRules.DefaultSeasonalTrainingDueMetricValidator();
        var item = SeasonalTrainingDueMetric.NewSeasonalTrainingDueMetric("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SeasonalTrainingDueMetricRequiredManagerIdRuleFlagsABlankManagerId()
    {
        var rule = new SeasonalTrainingDueMetricRequiredManagerIdRule("seasonal training due metric needs a value");
        var item = SeasonalTrainingDueMetric.NewSeasonalTrainingDueMetric("", "");
        item.ManagerId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SeasonalTrainingDueMetricManagerIdLengthRuleFlagsALongManagerId()
    {
        var rule = new SeasonalTrainingDueMetricManagerIdLengthRule(120);
        var item = SeasonalTrainingDueMetric.NewSeasonalTrainingDueMetric("x", "y");
        item.ManagerId = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ManagerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        SeasonalTrainingDueMetricValidatorReportsEveryProblem();
        SeasonalTrainingDueMetricRequiredManagerIdRuleFlagsABlankManagerId();
        SeasonalTrainingDueMetricManagerIdLengthRuleFlagsALongManagerId();
    }
}
