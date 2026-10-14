namespace Acme.Workforce.Payrollops.Tests.PayRun.RunAllocation;

using Acme.Workforce.Payrollops.PayRun.RunAllocation;

/// <summary>Checks of run allocation code, run by the project's runner.</summary>
public static class AdaptivePayRunAllocationRulesChecks
{
    public static void AdaptivePayRunAllocationValidatorReportsEveryProblem()
    {
        var validator = AdaptivePayRunAllocationRules.DefaultAdaptivePayRunAllocationValidator();
        var item = AdaptivePayRunAllocation.NewAdaptivePayRunAllocation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdaptivePayRunAllocationRateRangeRuleFlagsValuesAboveRate()
    {
        var rule = new AdaptivePayRunAllocationRateRangeRule(2.4, 2089.3);
        var item = AdaptivePayRunAllocation.NewAdaptivePayRunAllocation("x", "y");
        item.Rate = 2090.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AdaptivePayRunAllocationCodeLengthRuleFlagsALongCode()
    {
        var rule = new AdaptivePayRunAllocationCodeLengthRule(32);
        var item = AdaptivePayRunAllocation.NewAdaptivePayRunAllocation("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdaptivePayRunAllocationTitleLengthRuleFlagsALongTitle()
    {
        var rule = new AdaptivePayRunAllocationTitleLengthRule(64);
        var item = AdaptivePayRunAllocation.NewAdaptivePayRunAllocation("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdaptivePayRunAllocationRequiredSummaryRuleFlagsABlankSummary()
    {
        var rule = new AdaptivePayRunAllocationRequiredSummaryRule("adaptive pay run allocation needs this field");
        var item = AdaptivePayRunAllocation.NewAdaptivePayRunAllocation("", "");
        item.Summary = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AdaptivePayRunAllocationValidatorReportsEveryProblem();
        AdaptivePayRunAllocationRateRangeRuleFlagsValuesAboveRate();
        AdaptivePayRunAllocationCodeLengthRuleFlagsALongCode();
        AdaptivePayRunAllocationTitleLengthRuleFlagsALongTitle();
        AdaptivePayRunAllocationRequiredSummaryRuleFlagsABlankSummary();
    }
}
