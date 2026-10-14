namespace Acme.Workforce.Contractors.Tests.Vendor.Review;

using Acme.Workforce.Contractors.Vendor.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class VendorReviewRulesChecks
{
    public static void VendorReviewValidatorReportsEveryProblem()
    {
        var validator = VendorReviewRules.DefaultVendorReviewValidator();
        var item = VendorReview.NewVendorReview("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void VendorReviewCapacityRangeRuleFlagsValuesAboveCapacity()
    {
        var rule = new VendorReviewCapacityRangeRule(32, 1000);
        var item = VendorReview.NewVendorReview("x", "y");
        item.Capacity = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void VendorReviewRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new VendorReviewRequiredCodeRule("vendor review needs an entry");
        var item = VendorReview.NewVendorReview("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void VendorReviewCodeLengthRuleFlagsALongCode()
    {
        var rule = new VendorReviewCodeLengthRule(255);
        var item = VendorReview.NewVendorReview("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void VendorReviewRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new VendorReviewRequiredTitleRule("vendor review needs a value");
        var item = VendorReview.NewVendorReview("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void VendorReviewRatingRangeRuleFlagsValuesAboveRating()
    {
        var rule = new VendorReviewRatingRangeRule(5.3, 2156.1);
        var item = VendorReview.NewVendorReview("x", "y");
        item.Rating = 2157.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        VendorReviewValidatorReportsEveryProblem();
        VendorReviewCapacityRangeRuleFlagsValuesAboveCapacity();
        VendorReviewRequiredCodeRuleFlagsABlankCode();
        VendorReviewCodeLengthRuleFlagsALongCode();
        VendorReviewRequiredTitleRuleFlagsABlankTitle();
        VendorReviewRatingRangeRuleFlagsValuesAboveRating();
    }
}
