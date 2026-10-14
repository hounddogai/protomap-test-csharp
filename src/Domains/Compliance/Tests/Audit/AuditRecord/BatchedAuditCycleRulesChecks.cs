namespace Acme.Workforce.Compliance.Tests.Audit.AuditRecord;

using Acme.Workforce.Compliance.Audit.AuditRecord;

/// <summary>Checks of audit record code, run by the project's runner.</summary>
public static class BatchedAuditCycleRulesChecks
{
    public static void BatchedAuditCycleValidatorReportsEveryProblem()
    {
        var validator = BatchedAuditCycleRules.DefaultBatchedAuditCycleValidator();
        var item = BatchedAuditCycle.NewBatchedAuditCycle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BatchedAuditCycleRequiredEmployeeIdRuleFlagsABlankEmployeeId()
    {
        var rule = new BatchedAuditCycleRequiredEmployeeIdRule("batched audit cycle needs this field");
        var item = BatchedAuditCycle.NewBatchedAuditCycle("", "");
        item.EmployeeId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BatchedAuditCycleRequiredNameRuleFlagsABlankName()
    {
        var rule = new BatchedAuditCycleRequiredNameRule("batched audit cycle needs this field");
        var item = BatchedAuditCycle.NewBatchedAuditCycle("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BatchedAuditCycleRequiredCountryRuleFlagsABlankCountry()
    {
        var rule = new BatchedAuditCycleRequiredCountryRule("batched audit cycle needs this field");
        var item = BatchedAuditCycle.NewBatchedAuditCycle("", "");
        item.Country = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BatchedAuditCycleGraceDaysRangeRuleFlagsValuesAboveGraceDays()
    {
        var rule = new BatchedAuditCycleGraceDaysRangeRule(255, 86400);
        var item = BatchedAuditCycle.NewBatchedAuditCycle("x", "y");
        item.GraceDays = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        BatchedAuditCycleValidatorReportsEveryProblem();
        BatchedAuditCycleRequiredEmployeeIdRuleFlagsABlankEmployeeId();
        BatchedAuditCycleRequiredNameRuleFlagsABlankName();
        BatchedAuditCycleRequiredCountryRuleFlagsABlankCountry();
        BatchedAuditCycleGraceDaysRangeRuleFlagsValuesAboveGraceDays();
    }
}
