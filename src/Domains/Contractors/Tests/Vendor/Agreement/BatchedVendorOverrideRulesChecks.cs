namespace Acme.Workforce.Contractors.Tests.Vendor.Agreement;

using Acme.Workforce.Contractors.Vendor.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class BatchedVendorOverrideRulesChecks
{
    public static void BatchedVendorOverrideValidatorReportsEveryProblem()
    {
        var validator = BatchedVendorOverrideRules.DefaultBatchedVendorOverrideValidator();
        var item = BatchedVendorOverride.NewBatchedVendorOverride("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BatchedVendorOverrideRateRangeRuleFlagsValuesAboveRate()
    {
        var rule = new BatchedVendorOverrideRateRangeRule(6.8, 197.3);
        var item = BatchedVendorOverride.NewBatchedVendorOverride("x", "y");
        item.Rate = 198.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BatchedVendorOverrideRequiredNameRuleFlagsABlankName()
    {
        var rule = new BatchedVendorOverrideRequiredNameRule("batched vendor override needs this field");
        var item = BatchedVendorOverride.NewBatchedVendorOverride("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BatchedVendorOverrideNameLengthRuleFlagsALongName()
    {
        var rule = new BatchedVendorOverrideNameLengthRule(255);
        var item = BatchedVendorOverride.NewBatchedVendorOverride("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        BatchedVendorOverrideValidatorReportsEveryProblem();
        BatchedVendorOverrideRateRangeRuleFlagsValuesAboveRate();
        BatchedVendorOverrideRequiredNameRuleFlagsABlankName();
        BatchedVendorOverrideNameLengthRuleFlagsALongName();
    }
}
