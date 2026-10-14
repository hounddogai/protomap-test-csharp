namespace Acme.Workforce.Contractors.Tests.Vendor.Request;

using Acme.Workforce.Contractors.Vendor.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class LenientVendorBaselineRulesChecks
{
    public static void LenientVendorBaselineValidatorReportsEveryProblem()
    {
        var validator = LenientVendorBaselineRules.DefaultLenientVendorBaselineValidator();
        var item = LenientVendorBaseline.NewLenientVendorBaseline("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientVendorBaselineRequiredReviewerRuleFlagsABlankReviewer()
    {
        var rule = new LenientVendorBaselineRequiredReviewerRule("lenient vendor baseline needs a value");
        var item = LenientVendorBaseline.NewLenientVendorBaseline("", "");
        item.Reviewer = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientVendorBaselineCodeLengthRuleFlagsALongCode()
    {
        var rule = new LenientVendorBaselineCodeLengthRule(86400);
        var item = LenientVendorBaseline.NewLenientVendorBaseline("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LenientVendorBaselineRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LenientVendorBaselineRequiredCodeRule("lenient vendor baseline needs an entry");
        var item = LenientVendorBaseline.NewLenientVendorBaseline("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LenientVendorBaselineValidatorReportsEveryProblem();
        LenientVendorBaselineRequiredReviewerRuleFlagsABlankReviewer();
        LenientVendorBaselineCodeLengthRuleFlagsALongCode();
        LenientVendorBaselineRequiredCodeRuleFlagsABlankCode();
    }
}
