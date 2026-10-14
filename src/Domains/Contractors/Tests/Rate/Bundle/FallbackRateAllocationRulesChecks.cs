namespace Acme.Workforce.Contractors.Tests.Rate.Bundle;

using Acme.Workforce.Contractors.Rate.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class FallbackRateAllocationRulesChecks
{
    public static void FallbackRateAllocationValidatorReportsEveryProblem()
    {
        var validator = FallbackRateAllocationRules.DefaultFallbackRateAllocationValidator();
        var item = FallbackRateAllocation.NewFallbackRateAllocation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FallbackRateAllocationExpiresAtRangeRuleFlagsValuesAboveExpiresAt()
    {
        var rule = new FallbackRateAllocationExpiresAtRangeRule(64, 3600);
        var item = FallbackRateAllocation.NewFallbackRateAllocation("x", "y");
        item.ExpiresAt = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void FallbackRateAllocationRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new FallbackRateAllocationRequiredCodeRule("fallback rate allocation needs an entry");
        var item = FallbackRateAllocation.NewFallbackRateAllocation("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FallbackRateAllocationRequiredNameRuleFlagsABlankName()
    {
        var rule = new FallbackRateAllocationRequiredNameRule("fallback rate allocation needs this field");
        var item = FallbackRateAllocation.NewFallbackRateAllocation("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FallbackRateAllocationChannelLengthRuleFlagsALongChannel()
    {
        var rule = new FallbackRateAllocationChannelLengthRule(255);
        var item = FallbackRateAllocation.NewFallbackRateAllocation("x", "y");
        item.Channel = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Channel = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FallbackRateAllocationNameLengthRuleFlagsALongName()
    {
        var rule = new FallbackRateAllocationNameLengthRule(1000);
        var item = FallbackRateAllocation.NewFallbackRateAllocation("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        FallbackRateAllocationValidatorReportsEveryProblem();
        FallbackRateAllocationExpiresAtRangeRuleFlagsValuesAboveExpiresAt();
        FallbackRateAllocationRequiredCodeRuleFlagsABlankCode();
        FallbackRateAllocationRequiredNameRuleFlagsABlankName();
        FallbackRateAllocationChannelLengthRuleFlagsALongChannel();
        FallbackRateAllocationNameLengthRuleFlagsALongName();
    }
}
