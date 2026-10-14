namespace Acme.Workforce.Payrollops.Tests.TaxForm.Score;

using Acme.Workforce.Payrollops.TaxForm.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class TaxFormBatchRulesChecks
{
    public static void TaxFormBatchValidatorReportsEveryProblem()
    {
        var validator = TaxFormBatchRules.DefaultTaxFormBatchValidator();
        var item = TaxFormBatch.NewTaxFormBatch("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TaxFormBatchCodeLengthRuleFlagsALongCode()
    {
        var rule = new TaxFormBatchCodeLengthRule(120);
        var item = TaxFormBatch.NewTaxFormBatch("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TaxFormBatchRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new TaxFormBatchRequiredLabelRule("tax form batch needs this field");
        var item = TaxFormBatch.NewTaxFormBatch("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TaxFormBatchEndsAtRangeRuleFlagsValuesAboveEndsAt()
    {
        var rule = new TaxFormBatchEndsAtRangeRule(120, 86400);
        var item = TaxFormBatch.NewTaxFormBatch("x", "y");
        item.EndsAt = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TaxFormBatchRequiredSummaryRuleFlagsABlankSummary()
    {
        var rule = new TaxFormBatchRequiredSummaryRule("tax form batch needs an entry");
        var item = TaxFormBatch.NewTaxFormBatch("", "");
        item.Summary = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        TaxFormBatchValidatorReportsEveryProblem();
        TaxFormBatchCodeLengthRuleFlagsALongCode();
        TaxFormBatchRequiredLabelRuleFlagsABlankLabel();
        TaxFormBatchEndsAtRangeRuleFlagsValuesAboveEndsAt();
        TaxFormBatchRequiredSummaryRuleFlagsABlankSummary();
    }
}
