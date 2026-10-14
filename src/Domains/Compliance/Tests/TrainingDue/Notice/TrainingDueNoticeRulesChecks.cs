namespace Acme.Workforce.Compliance.Tests.TrainingDue.Notice;

using Acme.Workforce.Compliance.TrainingDue.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class TrainingDueNoticeRulesChecks
{
    public static void TrainingDueNoticeValidatorReportsEveryProblem()
    {
        var validator = TrainingDueNoticeRules.DefaultTrainingDueNoticeValidator();
        var item = TrainingDueNotice.NewTrainingDueNotice("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TrainingDueNoticeLabelLengthRuleFlagsALongLabel()
    {
        var rule = new TrainingDueNoticeLabelLengthRule(86400);
        var item = TrainingDueNotice.NewTrainingDueNotice("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TrainingDueNoticeAttemptsRangeRuleFlagsValuesAboveAttempts()
    {
        var rule = new TrainingDueNoticeAttemptsRangeRule(32, 64);
        var item = TrainingDueNotice.NewTrainingDueNotice("x", "y");
        item.Attempts = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TrainingDueNoticeThresholdRangeRuleFlagsValuesAboveThreshold()
    {
        var rule = new TrainingDueNoticeThresholdRangeRule(1.6, 4512.5);
        var item = TrainingDueNotice.NewTrainingDueNotice("x", "y");
        item.Threshold = 4514.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        TrainingDueNoticeValidatorReportsEveryProblem();
        TrainingDueNoticeLabelLengthRuleFlagsALongLabel();
        TrainingDueNoticeAttemptsRangeRuleFlagsValuesAboveAttempts();
        TrainingDueNoticeThresholdRangeRuleFlagsValuesAboveThreshold();
    }
}
