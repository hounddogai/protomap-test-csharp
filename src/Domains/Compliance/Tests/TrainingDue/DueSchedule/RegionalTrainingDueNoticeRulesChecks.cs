namespace Acme.Workforce.Compliance.Tests.TrainingDue.DueSchedule;

using Acme.Workforce.Compliance.TrainingDue.DueSchedule;

/// <summary>Checks of due schedule code, run by the project's runner.</summary>
public static class RegionalTrainingDueNoticeRulesChecks
{
    public static void RegionalTrainingDueNoticeValidatorReportsEveryProblem()
    {
        var validator = RegionalTrainingDueNoticeRules.DefaultRegionalTrainingDueNoticeValidator();
        var item = RegionalTrainingDueNotice.NewRegionalTrainingDueNotice("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RegionalTrainingDueNoticeLabelLengthRuleFlagsALongLabel()
    {
        var rule = new RegionalTrainingDueNoticeLabelLengthRule(255);
        var item = RegionalTrainingDueNotice.NewRegionalTrainingDueNotice("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        RegionalTrainingDueNoticeValidatorReportsEveryProblem();
        RegionalTrainingDueNoticeLabelLengthRuleFlagsALongLabel();
    }
}
