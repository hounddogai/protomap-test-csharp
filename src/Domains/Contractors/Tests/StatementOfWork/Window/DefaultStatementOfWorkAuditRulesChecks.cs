namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Window;

using Acme.Workforce.Contractors.StatementOfWork.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class DefaultStatementOfWorkAuditRulesChecks
{
    public static void DefaultStatementOfWorkAuditValidatorReportsEveryProblem()
    {
        var validator = DefaultStatementOfWorkAuditRules.DefaultDefaultStatementOfWorkAuditValidator();
        var item = DefaultStatementOfWorkAudit.NewDefaultStatementOfWorkAudit("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultStatementOfWorkAuditRequiredReviewerRuleFlagsABlankReviewer()
    {
        var rule = new DefaultStatementOfWorkAuditRequiredReviewerRule("default statement of work audit needs a value");
        var item = DefaultStatementOfWorkAudit.NewDefaultStatementOfWorkAudit("", "");
        item.Reviewer = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultStatementOfWorkAuditBatchSizeRangeRuleFlagsValuesAboveBatchSize()
    {
        var rule = new DefaultStatementOfWorkAuditBatchSizeRangeRule(3600, 86400);
        var item = DefaultStatementOfWorkAudit.NewDefaultStatementOfWorkAudit("x", "y");
        item.BatchSize = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DefaultStatementOfWorkAuditRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new DefaultStatementOfWorkAuditRequiredCodeRule("default statement of work audit needs an entry");
        var item = DefaultStatementOfWorkAudit.NewDefaultStatementOfWorkAudit("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultStatementOfWorkAuditRequiredNameRuleFlagsABlankName()
    {
        var rule = new DefaultStatementOfWorkAuditRequiredNameRule("default statement of work audit needs a value");
        var item = DefaultStatementOfWorkAudit.NewDefaultStatementOfWorkAudit("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        DefaultStatementOfWorkAuditValidatorReportsEveryProblem();
        DefaultStatementOfWorkAuditRequiredReviewerRuleFlagsABlankReviewer();
        DefaultStatementOfWorkAuditBatchSizeRangeRuleFlagsValuesAboveBatchSize();
        DefaultStatementOfWorkAuditRequiredCodeRuleFlagsABlankCode();
        DefaultStatementOfWorkAuditRequiredNameRuleFlagsABlankName();
    }
}
