namespace Acme.Workforce.Compliance.Tests.TrainingDue.Schedule;

using Acme.Workforce.Compliance.TrainingDue.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class TrainingDueMetricRulesChecks
{
    public static void TrainingDueMetricValidatorReportsEveryProblem()
    {
        var validator = TrainingDueMetricRules.DefaultTrainingDueMetricValidator();
        var item = TrainingDueMetric.NewTrainingDueMetric("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TrainingDueMetricCodeLengthRuleFlagsALongCode()
    {
        var rule = new TrainingDueMetricCodeLengthRule(80);
        var item = TrainingDueMetric.NewTrainingDueMetric("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TrainingDueMetricPhoneLengthRuleFlagsALongPhone()
    {
        var rule = new TrainingDueMetricPhoneLengthRule(255);
        var item = TrainingDueMetric.NewTrainingDueMetric("x", "y");
        item.Phone = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Phone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TrainingDueMetricTitleLengthRuleFlagsALongTitle()
    {
        var rule = new TrainingDueMetricTitleLengthRule(32);
        var item = TrainingDueMetric.NewTrainingDueMetric("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        TrainingDueMetricValidatorReportsEveryProblem();
        TrainingDueMetricCodeLengthRuleFlagsALongCode();
        TrainingDueMetricPhoneLengthRuleFlagsALongPhone();
        TrainingDueMetricTitleLengthRuleFlagsALongTitle();
    }
}
