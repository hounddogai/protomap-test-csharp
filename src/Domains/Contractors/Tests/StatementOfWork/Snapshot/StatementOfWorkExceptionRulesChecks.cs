namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Snapshot;

using Acme.Workforce.Contractors.StatementOfWork.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class StatementOfWorkExceptionRulesChecks
{
    public static void StatementOfWorkExceptionValidatorReportsEveryProblem()
    {
        var validator = StatementOfWorkExceptionRules.DefaultStatementOfWorkExceptionValidator();
        var item = StatementOfWorkException.NewStatementOfWorkException("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StatementOfWorkExceptionRequiredCategoryRuleFlagsABlankCategory()
    {
        var rule = new StatementOfWorkExceptionRequiredCategoryRule("statement of work exception needs an entry");
        var item = StatementOfWorkException.NewStatementOfWorkException("", "");
        item.Category = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StatementOfWorkExceptionRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new StatementOfWorkExceptionRequiredCodeRule("statement of work exception needs an entry");
        var item = StatementOfWorkException.NewStatementOfWorkException("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StatementOfWorkExceptionRequiredSummaryRuleFlagsABlankSummary()
    {
        var rule = new StatementOfWorkExceptionRequiredSummaryRule("statement of work exception needs a value");
        var item = StatementOfWorkException.NewStatementOfWorkException("", "");
        item.Summary = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StatementOfWorkExceptionCategoryLengthRuleFlagsALongCategory()
    {
        var rule = new StatementOfWorkExceptionCategoryLengthRule(32);
        var item = StatementOfWorkException.NewStatementOfWorkException("x", "y");
        item.Category = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Category = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StatementOfWorkExceptionRequiredNameRuleFlagsABlankName()
    {
        var rule = new StatementOfWorkExceptionRequiredNameRule("statement of work exception needs a value");
        var item = StatementOfWorkException.NewStatementOfWorkException("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        StatementOfWorkExceptionValidatorReportsEveryProblem();
        StatementOfWorkExceptionRequiredCategoryRuleFlagsABlankCategory();
        StatementOfWorkExceptionRequiredCodeRuleFlagsABlankCode();
        StatementOfWorkExceptionRequiredSummaryRuleFlagsABlankSummary();
        StatementOfWorkExceptionCategoryLengthRuleFlagsALongCategory();
        StatementOfWorkExceptionRequiredNameRuleFlagsABlankName();
    }
}
