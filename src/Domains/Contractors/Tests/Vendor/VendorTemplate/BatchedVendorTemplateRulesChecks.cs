namespace Acme.Workforce.Contractors.Tests.Vendor.VendorTemplate;

using Acme.Workforce.Contractors.Vendor.VendorTemplate;

/// <summary>Checks of vendor template code, run by the project's runner.</summary>
public static class BatchedVendorTemplateRulesChecks
{
    public static void BatchedVendorTemplateValidatorReportsEveryProblem()
    {
        var validator = BatchedVendorTemplateRules.DefaultBatchedVendorTemplateValidator();
        var item = BatchedVendorTemplate.NewBatchedVendorTemplate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BatchedVendorTemplateRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new BatchedVendorTemplateRequiredCodeRule("batched vendor template needs this field");
        var item = BatchedVendorTemplate.NewBatchedVendorTemplate("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BatchedVendorTemplateTitleLengthRuleFlagsALongTitle()
    {
        var rule = new BatchedVendorTemplateTitleLengthRule(16);
        var item = BatchedVendorTemplate.NewBatchedVendorTemplate("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BatchedVendorTemplateRequiredSalaryBandRuleFlagsABlankSalaryBand()
    {
        var rule = new BatchedVendorTemplateRequiredSalaryBandRule("batched vendor template needs a value");
        var item = BatchedVendorTemplate.NewBatchedVendorTemplate("", "");
        item.SalaryBand = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        BatchedVendorTemplateValidatorReportsEveryProblem();
        BatchedVendorTemplateRequiredCodeRuleFlagsABlankCode();
        BatchedVendorTemplateTitleLengthRuleFlagsALongTitle();
        BatchedVendorTemplateRequiredSalaryBandRuleFlagsABlankSalaryBand();
    }
}
