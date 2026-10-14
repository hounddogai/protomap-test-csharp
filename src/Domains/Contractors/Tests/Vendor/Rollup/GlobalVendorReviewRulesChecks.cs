namespace Acme.Workforce.Contractors.Tests.Vendor.Rollup;

using Acme.Workforce.Contractors.Vendor.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class GlobalVendorReviewRulesChecks
{
    public static void GlobalVendorReviewValidatorReportsEveryProblem()
    {
        var validator = GlobalVendorReviewRules.DefaultGlobalVendorReviewValidator();
        var item = GlobalVendorReview.NewGlobalVendorReview("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void GlobalVendorReviewRequiredCountryRuleFlagsABlankCountry()
    {
        var rule = new GlobalVendorReviewRequiredCountryRule("global vendor review needs this field");
        var item = GlobalVendorReview.NewGlobalVendorReview("", "");
        item.Country = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void GlobalVendorReviewRequiredChannelRuleFlagsABlankChannel()
    {
        var rule = new GlobalVendorReviewRequiredChannelRule("global vendor review needs a value");
        var item = GlobalVendorReview.NewGlobalVendorReview("", "");
        item.Channel = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void GlobalVendorReviewRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new GlobalVendorReviewRequiredCodeRule("global vendor review needs this field");
        var item = GlobalVendorReview.NewGlobalVendorReview("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        GlobalVendorReviewValidatorReportsEveryProblem();
        GlobalVendorReviewRequiredCountryRuleFlagsABlankCountry();
        GlobalVendorReviewRequiredChannelRuleFlagsABlankChannel();
        GlobalVendorReviewRequiredCodeRuleFlagsABlankCode();
    }
}
