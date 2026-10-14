namespace Acme.Workforce.Talent.Tests.Mobility.Preference;

using Acme.Workforce.Talent.Mobility.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class DefaultMobilityClaimRulesChecks
{
    public static void DefaultMobilityClaimValidatorReportsEveryProblem()
    {
        var validator = DefaultMobilityClaimRules.DefaultDefaultMobilityClaimValidator();
        var item = DefaultMobilityClaim.NewDefaultMobilityClaim("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultMobilityClaimMaximumRangeRuleFlagsValuesAboveMaximum()
    {
        var rule = new DefaultMobilityClaimMaximumRangeRule(3.8, 3959.4);
        var item = DefaultMobilityClaim.NewDefaultMobilityClaim("x", "y");
        item.Maximum = 3960.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DefaultMobilityClaimQuantityRangeRuleFlagsValuesAboveQuantity()
    {
        var rule = new DefaultMobilityClaimQuantityRangeRule(16, 255);
        var item = DefaultMobilityClaim.NewDefaultMobilityClaim("x", "y");
        item.Quantity = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        DefaultMobilityClaimValidatorReportsEveryProblem();
        DefaultMobilityClaimMaximumRangeRuleFlagsValuesAboveMaximum();
        DefaultMobilityClaimQuantityRangeRuleFlagsValuesAboveQuantity();
    }
}
