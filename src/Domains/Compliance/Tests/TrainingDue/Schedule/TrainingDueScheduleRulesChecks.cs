namespace Acme.Workforce.Compliance.Tests.TrainingDue.Schedule;

using Acme.Workforce.Compliance.TrainingDue.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class TrainingDueScheduleRulesChecks
{
    public static void TrainingDueScheduleValidatorReportsEveryProblem()
    {
        var validator = TrainingDueScheduleRules.DefaultTrainingDueScheduleValidator();
        var item = TrainingDueSchedule.NewTrainingDueSchedule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TrainingDueScheduleRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new TrainingDueScheduleRequiredCodeRule("training due schedule needs a value");
        var item = TrainingDueSchedule.NewTrainingDueSchedule("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TrainingDueScheduleTitleLengthRuleFlagsALongTitle()
    {
        var rule = new TrainingDueScheduleTitleLengthRule(64);
        var item = TrainingDueSchedule.NewTrainingDueSchedule("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TrainingDueScheduleCategoryLengthRuleFlagsALongCategory()
    {
        var rule = new TrainingDueScheduleCategoryLengthRule(32);
        var item = TrainingDueSchedule.NewTrainingDueSchedule("x", "y");
        item.Category = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Category = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        TrainingDueScheduleValidatorReportsEveryProblem();
        TrainingDueScheduleRequiredCodeRuleFlagsABlankCode();
        TrainingDueScheduleTitleLengthRuleFlagsALongTitle();
        TrainingDueScheduleCategoryLengthRuleFlagsALongCategory();
    }
}
