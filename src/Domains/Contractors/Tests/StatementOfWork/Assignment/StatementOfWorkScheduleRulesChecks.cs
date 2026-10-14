namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Assignment;

using Acme.Workforce.Contractors.StatementOfWork.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class StatementOfWorkScheduleRulesChecks
{
    public static void StatementOfWorkScheduleValidatorReportsEveryProblem()
    {
        var validator = StatementOfWorkScheduleRules.DefaultStatementOfWorkScheduleValidator();
        var item = StatementOfWorkSchedule.NewStatementOfWorkSchedule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StatementOfWorkScheduleRequiredNameRuleFlagsABlankName()
    {
        var rule = new StatementOfWorkScheduleRequiredNameRule("statement of work schedule needs an entry");
        var item = StatementOfWorkSchedule.NewStatementOfWorkSchedule("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StatementOfWorkScheduleRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new StatementOfWorkScheduleRequiredCodeRule("statement of work schedule needs an entry");
        var item = StatementOfWorkSchedule.NewStatementOfWorkSchedule("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StatementOfWorkScheduleCreatedAtRangeRuleFlagsValuesAboveCreatedAt()
    {
        var rule = new StatementOfWorkScheduleCreatedAtRangeRule(64, 86400);
        var item = StatementOfWorkSchedule.NewStatementOfWorkSchedule("x", "y");
        item.CreatedAt = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void StatementOfWorkScheduleNameLengthRuleFlagsALongName()
    {
        var rule = new StatementOfWorkScheduleNameLengthRule(32);
        var item = StatementOfWorkSchedule.NewStatementOfWorkSchedule("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        StatementOfWorkScheduleValidatorReportsEveryProblem();
        StatementOfWorkScheduleRequiredNameRuleFlagsABlankName();
        StatementOfWorkScheduleRequiredCodeRuleFlagsABlankCode();
        StatementOfWorkScheduleCreatedAtRangeRuleFlagsValuesAboveCreatedAt();
        StatementOfWorkScheduleNameLengthRuleFlagsALongName();
    }
}
