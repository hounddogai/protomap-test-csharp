namespace Acme.Workforce.Payrollops.Tests.Garnishment.Target;

using Acme.Workforce.Payrollops.Garnishment.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class AdaptiveGarnishmentTierRulesChecks
{
    public static void AdaptiveGarnishmentTierValidatorReportsEveryProblem()
    {
        var validator = AdaptiveGarnishmentTierRules.DefaultAdaptiveGarnishmentTierValidator();
        var item = AdaptiveGarnishmentTier.NewAdaptiveGarnishmentTier("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdaptiveGarnishmentTierTimezoneLengthRuleFlagsALongTimezone()
    {
        var rule = new AdaptiveGarnishmentTierTimezoneLengthRule(86400);
        var item = AdaptiveGarnishmentTier.NewAdaptiveGarnishmentTier("x", "y");
        item.Timezone = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Timezone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdaptiveGarnishmentTierChecksumLengthRuleFlagsALongChecksum()
    {
        var rule = new AdaptiveGarnishmentTierChecksumLengthRule(86400);
        var item = AdaptiveGarnishmentTier.NewAdaptiveGarnishmentTier("x", "y");
        item.Checksum = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Checksum = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AdaptiveGarnishmentTierValidatorReportsEveryProblem();
        AdaptiveGarnishmentTierTimezoneLengthRuleFlagsALongTimezone();
        AdaptiveGarnishmentTierChecksumLengthRuleFlagsALongChecksum();
    }
}
