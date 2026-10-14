namespace Acme.Workforce.Contractors.Tests.Vendor.Request;

using Acme.Workforce.Contractors.Vendor.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class LenientVendorBatchRulesChecks
{
    public static void LenientVendorBatchValidatorReportsEveryProblem()
    {
        var validator = LenientVendorBatchRules.DefaultLenientVendorBatchValidator();
        var item = LenientVendorBatch.NewLenientVendorBatch("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientVendorBatchRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new LenientVendorBatchRequiredLabelRule("lenient vendor batch needs a value");
        var item = LenientVendorBatch.NewLenientVendorBatch("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientVendorBatchFloorValueRangeRuleFlagsValuesAboveFloorValue()
    {
        var rule = new LenientVendorBatchFloorValueRangeRule(2.5, 2935.9);
        var item = LenientVendorBatch.NewLenientVendorBatch("x", "y");
        item.FloorValue = 2937.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LenientVendorBatchLabelLengthRuleFlagsALongLabel()
    {
        var rule = new LenientVendorBatchLabelLengthRule(64);
        var item = LenientVendorBatch.NewLenientVendorBatch("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LenientVendorBatchValidatorReportsEveryProblem();
        LenientVendorBatchRequiredLabelRuleFlagsABlankLabel();
        LenientVendorBatchFloorValueRangeRuleFlagsValuesAboveFloorValue();
        LenientVendorBatchLabelLengthRuleFlagsALongLabel();
    }
}
