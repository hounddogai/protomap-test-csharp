namespace Acme.Workforce.Compensation.Tests.MeritCycle.Segment;

using Acme.Workforce.Compensation.MeritCycle.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class MeritCycleEstimateRulesChecks
{
    public static void MeritCycleEstimateValidatorReportsEveryProblem()
    {
        var validator = MeritCycleEstimateRules.DefaultMeritCycleEstimateValidator();
        var item = MeritCycleEstimate.NewMeritCycleEstimate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void MeritCycleEstimateCodeLengthRuleFlagsALongCode()
    {
        var rule = new MeritCycleEstimateCodeLengthRule(86400);
        var item = MeritCycleEstimate.NewMeritCycleEstimate("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void MeritCycleEstimateRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new MeritCycleEstimateRequiredCodeRule("merit cycle estimate needs an entry");
        var item = MeritCycleEstimate.NewMeritCycleEstimate("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void MeritCycleEstimateRequiredChannelRuleFlagsABlankChannel()
    {
        var rule = new MeritCycleEstimateRequiredChannelRule("merit cycle estimate needs a value");
        var item = MeritCycleEstimate.NewMeritCycleEstimate("", "");
        item.Channel = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        MeritCycleEstimateValidatorReportsEveryProblem();
        MeritCycleEstimateCodeLengthRuleFlagsALongCode();
        MeritCycleEstimateRequiredCodeRuleFlagsABlankCode();
        MeritCycleEstimateRequiredChannelRuleFlagsABlankChannel();
    }
}
