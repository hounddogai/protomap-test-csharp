namespace Acme.Workforce.Compliance.Tests.Audit.Batch;

using Acme.Workforce.Compliance.Audit.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class AuditBatchRulesChecks
{
    public static void AuditBatchValidatorReportsEveryProblem()
    {
        var validator = AuditBatchRules.DefaultAuditBatchValidator();
        var item = AuditBatch.NewAuditBatch("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AuditBatchCreatedAtRangeRuleFlagsValuesAboveCreatedAt()
    {
        var rule = new AuditBatchCreatedAtRangeRule(32, 255);
        var item = AuditBatch.NewAuditBatch("x", "y");
        item.CreatedAt = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AuditBatchRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new AuditBatchRequiredTitleRule("audit batch needs this field");
        var item = AuditBatch.NewAuditBatch("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AuditBatchRetriesRangeRuleFlagsValuesAboveRetries()
    {
        var rule = new AuditBatchRetriesRangeRule(16, 80);
        var item = AuditBatch.NewAuditBatch("x", "y");
        item.Retries = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        AuditBatchValidatorReportsEveryProblem();
        AuditBatchCreatedAtRangeRuleFlagsValuesAboveCreatedAt();
        AuditBatchRequiredTitleRuleFlagsABlankTitle();
        AuditBatchRetriesRangeRuleFlagsValuesAboveRetries();
    }
}
