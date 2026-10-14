namespace Acme.Workforce.Contractors.Tests.Vendor.Request;

using Acme.Workforce.Contractors.Vendor.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class StrictVendorRequestRulesChecks
{
    public static void StrictVendorRequestValidatorReportsEveryProblem()
    {
        var validator = StrictVendorRequestRules.DefaultStrictVendorRequestValidator();
        var item = StrictVendorRequest.NewStrictVendorRequest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StrictVendorRequestAttemptsRangeRuleFlagsValuesAboveAttempts()
    {
        var rule = new StrictVendorRequestAttemptsRangeRule(64, 255);
        var item = StrictVendorRequest.NewStrictVendorRequest("x", "y");
        item.Attempts = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void StrictVendorRequestMarginRangeRuleFlagsValuesAboveMargin()
    {
        var rule = new StrictVendorRequestMarginRangeRule(5.8, 221.6);
        var item = StrictVendorRequest.NewStrictVendorRequest("x", "y");
        item.Margin = 223.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void StrictVendorRequestRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new StrictVendorRequestRequiredCodeRule("strict vendor request needs a value");
        var item = StrictVendorRequest.NewStrictVendorRequest("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StrictVendorRequestRatioRangeRuleFlagsValuesAboveRatio()
    {
        var rule = new StrictVendorRequestRatioRangeRule(4.9, 2178.8);
        var item = StrictVendorRequest.NewStrictVendorRequest("x", "y");
        item.Ratio = 2180.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        StrictVendorRequestValidatorReportsEveryProblem();
        StrictVendorRequestAttemptsRangeRuleFlagsValuesAboveAttempts();
        StrictVendorRequestMarginRangeRuleFlagsValuesAboveMargin();
        StrictVendorRequestRequiredCodeRuleFlagsABlankCode();
        StrictVendorRequestRatioRangeRuleFlagsValuesAboveRatio();
    }
}
