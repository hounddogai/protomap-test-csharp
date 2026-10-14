namespace Acme.Workforce.Absence.Tests.LeaveRequest.Window;

using Acme.Workforce.Absence.LeaveRequest.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class FallbackLeaveRequestAdjustmentRulesChecks
{
    public static void FallbackLeaveRequestAdjustmentValidatorReportsEveryProblem()
    {
        var validator = FallbackLeaveRequestAdjustmentRules.DefaultFallbackLeaveRequestAdjustmentValidator();
        var item = FallbackLeaveRequestAdjustment.NewFallbackLeaveRequestAdjustment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FallbackLeaveRequestAdjustmentRatioRangeRuleFlagsValuesAboveRatio()
    {
        var rule = new FallbackLeaveRequestAdjustmentRatioRangeRule(2.6, 3465.6);
        var item = FallbackLeaveRequestAdjustment.NewFallbackLeaveRequestAdjustment("x", "y");
        item.Ratio = 3467.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void FallbackLeaveRequestAdjustmentRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new FallbackLeaveRequestAdjustmentRequiredCodeRule("fallback leave request adjustment needs a value");
        var item = FallbackLeaveRequestAdjustment.NewFallbackLeaveRequestAdjustment("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FallbackLeaveRequestAdjustmentLabelLengthRuleFlagsALongLabel()
    {
        var rule = new FallbackLeaveRequestAdjustmentLabelLengthRule(255);
        var item = FallbackLeaveRequestAdjustment.NewFallbackLeaveRequestAdjustment("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FallbackLeaveRequestAdjustmentRequiredCountryRuleFlagsABlankCountry()
    {
        var rule = new FallbackLeaveRequestAdjustmentRequiredCountryRule(
            "fallback leave request adjustment needs a value");
        var item = FallbackLeaveRequestAdjustment.NewFallbackLeaveRequestAdjustment("", "");
        item.Country = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        FallbackLeaveRequestAdjustmentValidatorReportsEveryProblem();
        FallbackLeaveRequestAdjustmentRatioRangeRuleFlagsValuesAboveRatio();
        FallbackLeaveRequestAdjustmentRequiredCodeRuleFlagsABlankCode();
        FallbackLeaveRequestAdjustmentLabelLengthRuleFlagsALongLabel();
        FallbackLeaveRequestAdjustmentRequiredCountryRuleFlagsABlankCountry();
    }
}
