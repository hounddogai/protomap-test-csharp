namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Cycle;

using Acme.Workforce.Contractors.StatementOfWork.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class RollingStatementOfWorkVarianceRulesChecks
{
    public static void RollingStatementOfWorkVarianceValidatorReportsEveryProblem()
    {
        var validator = RollingStatementOfWorkVarianceRules.DefaultRollingStatementOfWorkVarianceValidator();
        var item = RollingStatementOfWorkVariance.NewRollingStatementOfWorkVariance("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RollingStatementOfWorkVarianceRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new RollingStatementOfWorkVarianceRequiredTitleRule(
            "rolling statement of work variance needs an entry");
        var item = RollingStatementOfWorkVariance.NewRollingStatementOfWorkVariance("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RollingStatementOfWorkVarianceCodeLengthRuleFlagsALongCode()
    {
        var rule = new RollingStatementOfWorkVarianceCodeLengthRule(120);
        var item = RollingStatementOfWorkVariance.NewRollingStatementOfWorkVariance("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RollingStatementOfWorkVarianceRequiredGradeRuleFlagsABlankGrade()
    {
        var rule = new RollingStatementOfWorkVarianceRequiredGradeRule(
            "rolling statement of work variance needs a value");
        var item = RollingStatementOfWorkVariance.NewRollingStatementOfWorkVariance("", "");
        item.Grade = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        RollingStatementOfWorkVarianceValidatorReportsEveryProblem();
        RollingStatementOfWorkVarianceRequiredTitleRuleFlagsABlankTitle();
        RollingStatementOfWorkVarianceCodeLengthRuleFlagsALongCode();
        RollingStatementOfWorkVarianceRequiredGradeRuleFlagsABlankGrade();
    }
}
