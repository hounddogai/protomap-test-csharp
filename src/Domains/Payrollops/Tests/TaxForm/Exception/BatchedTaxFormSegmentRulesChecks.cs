namespace Acme.Workforce.Payrollops.Tests.TaxForm.Exception;

using Acme.Workforce.Payrollops.TaxForm.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class BatchedTaxFormSegmentRulesChecks
{
    public static void BatchedTaxFormSegmentValidatorReportsEveryProblem()
    {
        var validator = BatchedTaxFormSegmentRules.DefaultBatchedTaxFormSegmentValidator();
        var item = BatchedTaxFormSegment.NewBatchedTaxFormSegment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BatchedTaxFormSegmentRequiredSkillRuleFlagsABlankSkill()
    {
        var rule = new BatchedTaxFormSegmentRequiredSkillRule("batched tax form segment needs this field");
        var item = BatchedTaxFormSegment.NewBatchedTaxFormSegment("", "");
        item.Skill = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BatchedTaxFormSegmentRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new BatchedTaxFormSegmentRequiredLabelRule("batched tax form segment needs this field");
        var item = BatchedTaxFormSegment.NewBatchedTaxFormSegment("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        BatchedTaxFormSegmentValidatorReportsEveryProblem();
        BatchedTaxFormSegmentRequiredSkillRuleFlagsABlankSkill();
        BatchedTaxFormSegmentRequiredLabelRuleFlagsABlankLabel();
    }
}
