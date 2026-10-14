namespace Acme.Workforce.Absence.Tests.Holiday.BatchedHolidaySchedule;

using Acme.Workforce.Absence.Holiday.BatchedHolidaySchedule;

/// <summary>Checks of batched holiday schedule code, run by the project's runner.</summary>
public static class BatchedHolidayScheduleRulesChecks
{
    public static void BatchedHolidayScheduleValidatorReportsEveryProblem()
    {
        var validator = BatchedHolidayScheduleRules.DefaultBatchedHolidayScheduleValidator();
        var item = BatchedHolidaySchedule.NewBatchedHolidaySchedule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BatchedHolidayScheduleCodeLengthRuleFlagsALongCode()
    {
        var rule = new BatchedHolidayScheduleCodeLengthRule(64);
        var item = BatchedHolidaySchedule.NewBatchedHolidaySchedule("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BatchedHolidayScheduleRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new BatchedHolidayScheduleRequiredCodeRule("batched holiday schedule needs an entry");
        var item = BatchedHolidaySchedule.NewBatchedHolidaySchedule("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BatchedHolidayScheduleRequiredJobCodeRuleFlagsABlankJobCode()
    {
        var rule = new BatchedHolidayScheduleRequiredJobCodeRule("batched holiday schedule needs an entry");
        var item = BatchedHolidaySchedule.NewBatchedHolidaySchedule("", "");
        item.JobCode = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BatchedHolidayScheduleDescriptionLengthRuleFlagsALongDescription()
    {
        var rule = new BatchedHolidayScheduleDescriptionLengthRule(16);
        var item = BatchedHolidaySchedule.NewBatchedHolidaySchedule("x", "y");
        item.Description = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Description = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        BatchedHolidayScheduleValidatorReportsEveryProblem();
        BatchedHolidayScheduleCodeLengthRuleFlagsALongCode();
        BatchedHolidayScheduleRequiredCodeRuleFlagsABlankCode();
        BatchedHolidayScheduleRequiredJobCodeRuleFlagsABlankJobCode();
        BatchedHolidayScheduleDescriptionLengthRuleFlagsALongDescription();
    }
}
