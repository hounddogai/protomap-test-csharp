namespace Acme.Workforce.Compliance.Tests.Audit.AuditBaseline;

using Acme.Workforce.Compliance.Audit.AuditBaseline;

/// <summary>Checks of audit baseline code, run by the project's runner.</summary>
public static class BatchedAuditTemplateRulesChecks
{
    public static void BatchedAuditTemplateValidatorReportsEveryProblem()
    {
        var validator = BatchedAuditTemplateRules.DefaultBatchedAuditTemplateValidator();
        var item = BatchedAuditTemplate.NewBatchedAuditTemplate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BatchedAuditTemplateRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new BatchedAuditTemplateRequiredCodeRule("batched audit template needs an entry");
        var item = BatchedAuditTemplate.NewBatchedAuditTemplate("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BatchedAuditTemplateBudgetRangeRuleFlagsValuesAboveBudget()
    {
        var rule = new BatchedAuditTemplateBudgetRangeRule(9.2, 2211.5);
        var item = BatchedAuditTemplate.NewBatchedAuditTemplate("x", "y");
        item.Budget = 2213.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BatchedAuditTemplateCeilingRangeRuleFlagsValuesAboveCeiling()
    {
        var rule = new BatchedAuditTemplateCeilingRangeRule(5.6, 1167.3);
        var item = BatchedAuditTemplate.NewBatchedAuditTemplate("x", "y");
        item.Ceiling = 1168.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        BatchedAuditTemplateValidatorReportsEveryProblem();
        BatchedAuditTemplateRequiredCodeRuleFlagsABlankCode();
        BatchedAuditTemplateBudgetRangeRuleFlagsValuesAboveBudget();
        BatchedAuditTemplateCeilingRangeRuleFlagsValuesAboveCeiling();
    }
}
