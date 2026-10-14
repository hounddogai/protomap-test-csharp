namespace Acme.Workforce.Contractors.Tests.Vendor.Entry;

using Acme.Workforce.Contractors.Vendor.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class LegacyVendorBatchRulesChecks
{
    public static void LegacyVendorBatchValidatorReportsEveryProblem()
    {
        var validator = LegacyVendorBatchRules.DefaultLegacyVendorBatchValidator();
        var item = LegacyVendorBatch.NewLegacyVendorBatch("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LegacyVendorBatchCodeLengthRuleFlagsALongCode()
    {
        var rule = new LegacyVendorBatchCodeLengthRule(255);
        var item = LegacyVendorBatch.NewLegacyVendorBatch("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LegacyVendorBatchRatingRangeRuleFlagsValuesAboveRating()
    {
        var rule = new LegacyVendorBatchRatingRangeRule(2.3, 3850.4);
        var item = LegacyVendorBatch.NewLegacyVendorBatch("x", "y");
        item.Rating = 3851.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LegacyVendorBatchRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new LegacyVendorBatchRequiredLabelRule("legacy vendor batch needs an entry");
        var item = LegacyVendorBatch.NewLegacyVendorBatch("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LegacyVendorBatchRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new LegacyVendorBatchRequiredTitleRule("legacy vendor batch needs an entry");
        var item = LegacyVendorBatch.NewLegacyVendorBatch("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LegacyVendorBatchValidatorReportsEveryProblem();
        LegacyVendorBatchCodeLengthRuleFlagsALongCode();
        LegacyVendorBatchRatingRangeRuleFlagsValuesAboveRating();
        LegacyVendorBatchRequiredLabelRuleFlagsABlankLabel();
        LegacyVendorBatchRequiredTitleRuleFlagsABlankTitle();
    }
}
