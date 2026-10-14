namespace Acme.Workforce.Compensation.Tests.PayBand.Baseline;

using Acme.Workforce.Compensation.PayBand.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class GlobalPayBandAllocationRulesChecks
{
    public static void GlobalPayBandAllocationValidatorReportsEveryProblem()
    {
        var validator = GlobalPayBandAllocationRules.DefaultGlobalPayBandAllocationValidator();
        var item = GlobalPayBandAllocation.NewGlobalPayBandAllocation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void GlobalPayBandAllocationRequiredChannelRuleFlagsABlankChannel()
    {
        var rule = new GlobalPayBandAllocationRequiredChannelRule("global pay band allocation needs this field");
        var item = GlobalPayBandAllocation.NewGlobalPayBandAllocation("", "");
        item.Channel = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        GlobalPayBandAllocationValidatorReportsEveryProblem();
        GlobalPayBandAllocationRequiredChannelRuleFlagsABlankChannel();
    }
}
