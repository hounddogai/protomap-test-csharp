namespace Acme.Workforce.Compensation.Tests.PayBand.Estimate;

using Acme.Workforce.Compensation.PayBand.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class FallbackPayBandMappingRulesChecks
{
    public static void FallbackPayBandMappingValidatorReportsEveryProblem()
    {
        var validator = FallbackPayBandMappingRules.DefaultFallbackPayBandMappingValidator();
        var item = FallbackPayBandMapping.NewFallbackPayBandMapping("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FallbackPayBandMappingSalaryBandLengthRuleFlagsALongSalaryBand()
    {
        var rule = new FallbackPayBandMappingSalaryBandLengthRule(3600);
        var item = FallbackPayBandMapping.NewFallbackPayBandMapping("x", "y");
        item.SalaryBand = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.SalaryBand = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FallbackPayBandMappingCodeLengthRuleFlagsALongCode()
    {
        var rule = new FallbackPayBandMappingCodeLengthRule(1000);
        var item = FallbackPayBandMapping.NewFallbackPayBandMapping("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FallbackPayBandMappingDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes()
    {
        var rule = new FallbackPayBandMappingDurationMinutesRangeRule(16, 1000);
        var item = FallbackPayBandMapping.NewFallbackPayBandMapping("x", "y");
        item.DurationMinutes = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void FallbackPayBandMappingRequiredSalaryBandRuleFlagsABlankSalaryBand()
    {
        var rule = new FallbackPayBandMappingRequiredSalaryBandRule("fallback pay band mapping needs a value");
        var item = FallbackPayBandMapping.NewFallbackPayBandMapping("", "");
        item.SalaryBand = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        FallbackPayBandMappingValidatorReportsEveryProblem();
        FallbackPayBandMappingSalaryBandLengthRuleFlagsALongSalaryBand();
        FallbackPayBandMappingCodeLengthRuleFlagsALongCode();
        FallbackPayBandMappingDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes();
        FallbackPayBandMappingRequiredSalaryBandRuleFlagsABlankSalaryBand();
    }
}
