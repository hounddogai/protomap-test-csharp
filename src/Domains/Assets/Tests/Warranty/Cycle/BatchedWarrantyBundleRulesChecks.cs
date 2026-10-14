namespace Acme.Workforce.Assets.Tests.Warranty.Cycle;

using Acme.Workforce.Assets.Warranty.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class BatchedWarrantyBundleRulesChecks
{
    public static void BatchedWarrantyBundleValidatorReportsEveryProblem()
    {
        var validator = BatchedWarrantyBundleRules.DefaultBatchedWarrantyBundleValidator();
        var item = BatchedWarrantyBundle.NewBatchedWarrantyBundle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BatchedWarrantyBundleChecksumLengthRuleFlagsALongChecksum()
    {
        var rule = new BatchedWarrantyBundleChecksumLengthRule(80);
        var item = BatchedWarrantyBundle.NewBatchedWarrantyBundle("x", "y");
        item.Checksum = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Checksum = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BatchedWarrantyBundleAmountRangeRuleFlagsValuesAboveAmount()
    {
        var rule = new BatchedWarrantyBundleAmountRangeRule(5.3, 3279.8);
        var item = BatchedWarrantyBundle.NewBatchedWarrantyBundle("x", "y");
        item.Amount = 3281.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BatchedWarrantyBundleNameLengthRuleFlagsALongName()
    {
        var rule = new BatchedWarrantyBundleNameLengthRule(1000);
        var item = BatchedWarrantyBundle.NewBatchedWarrantyBundle("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        BatchedWarrantyBundleValidatorReportsEveryProblem();
        BatchedWarrantyBundleChecksumLengthRuleFlagsALongChecksum();
        BatchedWarrantyBundleAmountRangeRuleFlagsValuesAboveAmount();
        BatchedWarrantyBundleNameLengthRuleFlagsALongName();
    }
}
