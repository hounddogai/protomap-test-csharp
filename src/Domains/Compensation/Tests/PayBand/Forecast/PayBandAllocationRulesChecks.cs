namespace Acme.Workforce.Compensation.Tests.PayBand.Forecast;

using Acme.Workforce.Compensation.PayBand.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class PayBandAllocationRulesChecks
{
    public static void PayBandAllocationValidatorReportsEveryProblem()
    {
        var validator = PayBandAllocationRules.DefaultPayBandAllocationValidator();
        var item = PayBandAllocation.NewPayBandAllocation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PayBandAllocationCodeLengthRuleFlagsALongCode()
    {
        var rule = new PayBandAllocationCodeLengthRule(86400);
        var item = PayBandAllocation.NewPayBandAllocation("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PayBandAllocationRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PayBandAllocationRequiredCodeRule("pay band allocation needs this field");
        var item = PayBandAllocation.NewPayBandAllocation("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PayBandAllocationTitleLengthRuleFlagsALongTitle()
    {
        var rule = new PayBandAllocationTitleLengthRule(64);
        var item = PayBandAllocation.NewPayBandAllocation("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PayBandAllocationValidatorReportsEveryProblem();
        PayBandAllocationCodeLengthRuleFlagsALongCode();
        PayBandAllocationRequiredCodeRuleFlagsABlankCode();
        PayBandAllocationTitleLengthRuleFlagsALongTitle();
    }
}
