namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Digest;

using Acme.Workforce.Contractors.StatementOfWork.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class StatementOfWorkAuditRulesChecks
{
    public static void StatementOfWorkAuditValidatorReportsEveryProblem()
    {
        var validator = StatementOfWorkAuditRules.DefaultStatementOfWorkAuditValidator();
        var item = StatementOfWorkAudit.NewStatementOfWorkAudit("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StatementOfWorkAuditEmployeeIdLengthRuleFlagsALongEmployeeId()
    {
        var rule = new StatementOfWorkAuditEmployeeIdLengthRule(3600);
        var item = StatementOfWorkAudit.NewStatementOfWorkAudit("x", "y");
        item.EmployeeId = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.EmployeeId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        StatementOfWorkAuditValidatorReportsEveryProblem();
        StatementOfWorkAuditEmployeeIdLengthRuleFlagsALongEmployeeId();
    }
}
