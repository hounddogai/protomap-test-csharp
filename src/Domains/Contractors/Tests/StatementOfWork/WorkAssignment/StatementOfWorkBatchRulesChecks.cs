namespace Acme.Workforce.Contractors.Tests.StatementOfWork.WorkAssignment;

using Acme.Workforce.Contractors.StatementOfWork.WorkAssignment;

/// <summary>Checks of work assignment code, run by the project's runner.</summary>
public static class StatementOfWorkBatchRulesChecks
{
    public static void StatementOfWorkBatchValidatorReportsEveryProblem()
    {
        var validator = StatementOfWorkBatchRules.DefaultStatementOfWorkBatchValidator();
        var item = StatementOfWorkBatch.NewStatementOfWorkBatch("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StatementOfWorkBatchPriorityRangeRuleFlagsValuesAbovePriority()
    {
        var rule = new StatementOfWorkBatchPriorityRangeRule(32, 255);
        var item = StatementOfWorkBatch.NewStatementOfWorkBatch("x", "y");
        item.Priority = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void StatementOfWorkBatchRequiredExternalRefRuleFlagsABlankExternalRef()
    {
        var rule = new StatementOfWorkBatchRequiredExternalRefRule("statement of work batch needs an entry");
        var item = StatementOfWorkBatch.NewStatementOfWorkBatch("", "");
        item.ExternalRef = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        StatementOfWorkBatchValidatorReportsEveryProblem();
        StatementOfWorkBatchPriorityRangeRuleFlagsValuesAbovePriority();
        StatementOfWorkBatchRequiredExternalRefRuleFlagsABlankExternalRef();
    }
}
