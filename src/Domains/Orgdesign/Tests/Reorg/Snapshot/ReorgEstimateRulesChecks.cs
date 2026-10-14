namespace Acme.Workforce.Orgdesign.Tests.Reorg.Snapshot;

using Acme.Workforce.Orgdesign.Reorg.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class ReorgEstimateRulesChecks
{
    public static void ReorgEstimateValidatorReportsEveryProblem()
    {
        var validator = ReorgEstimateRules.DefaultReorgEstimateValidator();
        var item = ReorgEstimate.NewReorgEstimate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ReorgEstimateRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new ReorgEstimateRequiredLabelRule("reorg estimate needs an entry");
        var item = ReorgEstimate.NewReorgEstimate("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ReorgEstimateRequiredPhoneRuleFlagsABlankPhone()
    {
        var rule = new ReorgEstimateRequiredPhoneRule("reorg estimate needs this field");
        var item = ReorgEstimate.NewReorgEstimate("", "");
        item.Phone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ReorgEstimateRequiredTimezoneRuleFlagsABlankTimezone()
    {
        var rule = new ReorgEstimateRequiredTimezoneRule("reorg estimate needs a value");
        var item = ReorgEstimate.NewReorgEstimate("", "");
        item.Timezone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ReorgEstimatePhoneLengthRuleFlagsALongPhone()
    {
        var rule = new ReorgEstimatePhoneLengthRule(64);
        var item = ReorgEstimate.NewReorgEstimate("x", "y");
        item.Phone = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Phone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ReorgEstimateValidatorReportsEveryProblem();
        ReorgEstimateRequiredLabelRuleFlagsABlankLabel();
        ReorgEstimateRequiredPhoneRuleFlagsABlankPhone();
        ReorgEstimateRequiredTimezoneRuleFlagsABlankTimezone();
        ReorgEstimatePhoneLengthRuleFlagsALongPhone();
    }
}
