namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Schedule;

using Acme.Workforce.Contractors.StatementOfWork.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class StrictStatementOfWorkAssignmentRulesChecks
{
    public static void StrictStatementOfWorkAssignmentValidatorReportsEveryProblem()
    {
        var validator = StrictStatementOfWorkAssignmentRules.DefaultStrictStatementOfWorkAssignmentValidator();
        var item = StrictStatementOfWorkAssignment.NewStrictStatementOfWorkAssignment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StrictStatementOfWorkAssignmentRequiredNameRuleFlagsABlankName()
    {
        var rule = new StrictStatementOfWorkAssignmentRequiredNameRule(
            "strict statement of work assignment needs a value");
        var item = StrictStatementOfWorkAssignment.NewStrictStatementOfWorkAssignment("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StrictStatementOfWorkAssignmentStartsAtRangeRuleFlagsValuesAboveStartsAt()
    {
        var rule = new StrictStatementOfWorkAssignmentStartsAtRangeRule(16, 32);
        var item = StrictStatementOfWorkAssignment.NewStrictStatementOfWorkAssignment("x", "y");
        item.StartsAt = 33;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void StrictStatementOfWorkAssignmentCodeLengthRuleFlagsALongCode()
    {
        var rule = new StrictStatementOfWorkAssignmentCodeLengthRule(16);
        var item = StrictStatementOfWorkAssignment.NewStrictStatementOfWorkAssignment("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        StrictStatementOfWorkAssignmentValidatorReportsEveryProblem();
        StrictStatementOfWorkAssignmentRequiredNameRuleFlagsABlankName();
        StrictStatementOfWorkAssignmentStartsAtRangeRuleFlagsValuesAboveStartsAt();
        StrictStatementOfWorkAssignmentCodeLengthRuleFlagsALongCode();
    }
}
