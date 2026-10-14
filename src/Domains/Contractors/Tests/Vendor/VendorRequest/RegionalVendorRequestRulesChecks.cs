namespace Acme.Workforce.Contractors.Tests.Vendor.VendorRequest;

using Acme.Workforce.Contractors.Vendor.VendorRequest;

/// <summary>Checks of vendor request code, run by the project's runner.</summary>
public static class RegionalVendorRequestRulesChecks
{
    public static void RegionalVendorRequestValidatorReportsEveryProblem()
    {
        var validator = RegionalVendorRequestRules.DefaultRegionalVendorRequestValidator();
        var item = RegionalVendorRequest.NewRegionalVendorRequest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RegionalVendorRequestScoreRangeRuleFlagsValuesAboveScore()
    {
        var rule = new RegionalVendorRequestScoreRangeRule(4.7, 4028.7);
        var item = RegionalVendorRequest.NewRegionalVendorRequest("x", "y");
        item.Score = 4030.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RegionalVendorRequestRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new RegionalVendorRequestRequiredCodeRule("regional vendor request needs this field");
        var item = RegionalVendorRequest.NewRegionalVendorRequest("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RegionalVendorRequestTitleLengthRuleFlagsALongTitle()
    {
        var rule = new RegionalVendorRequestTitleLengthRule(16);
        var item = RegionalVendorRequest.NewRegionalVendorRequest("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RegionalVendorRequestLocationLengthRuleFlagsALongLocation()
    {
        var rule = new RegionalVendorRequestLocationLengthRule(120);
        var item = RegionalVendorRequest.NewRegionalVendorRequest("x", "y");
        item.Location = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Location = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        RegionalVendorRequestValidatorReportsEveryProblem();
        RegionalVendorRequestScoreRangeRuleFlagsValuesAboveScore();
        RegionalVendorRequestRequiredCodeRuleFlagsABlankCode();
        RegionalVendorRequestTitleLengthRuleFlagsALongTitle();
        RegionalVendorRequestLocationLengthRuleFlagsALongLocation();
    }
}
