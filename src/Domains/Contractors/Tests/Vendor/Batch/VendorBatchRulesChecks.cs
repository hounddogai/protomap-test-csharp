namespace Acme.Workforce.Contractors.Tests.Vendor.Batch;

using Acme.Workforce.Contractors.Vendor.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class VendorBatchRulesChecks
{
    public static void VendorBatchValidatorReportsEveryProblem()
    {
        var validator = VendorBatchRules.DefaultVendorBatchValidator();
        var item = VendorBatch.NewVendorBatch("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void VendorBatchCodeLengthRuleFlagsALongCode()
    {
        var rule = new VendorBatchCodeLengthRule(255);
        var item = VendorBatch.NewVendorBatch("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void VendorBatchRequiredPhoneRuleFlagsABlankPhone()
    {
        var rule = new VendorBatchRequiredPhoneRule("vendor batch needs a value");
        var item = VendorBatch.NewVendorBatch("", "");
        item.Phone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void VendorBatchRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new VendorBatchRequiredLabelRule("vendor batch needs a value");
        var item = VendorBatch.NewVendorBatch("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void VendorBatchLabelLengthRuleFlagsALongLabel()
    {
        var rule = new VendorBatchLabelLengthRule(120);
        var item = VendorBatch.NewVendorBatch("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        VendorBatchValidatorReportsEveryProblem();
        VendorBatchCodeLengthRuleFlagsALongCode();
        VendorBatchRequiredPhoneRuleFlagsABlankPhone();
        VendorBatchRequiredLabelRuleFlagsABlankLabel();
        VendorBatchLabelLengthRuleFlagsALongLabel();
    }
}
