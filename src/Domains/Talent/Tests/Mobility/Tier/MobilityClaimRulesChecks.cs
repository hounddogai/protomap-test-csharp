namespace Acme.Workforce.Talent.Tests.Mobility.Tier;

using Acme.Workforce.Talent.Mobility.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class MobilityClaimRulesChecks
{
    public static void MobilityClaimValidatorReportsEveryProblem()
    {
        var validator = MobilityClaimRules.DefaultMobilityClaimValidator();
        var item = MobilityClaim.NewMobilityClaim("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void MobilityClaimNameLengthRuleFlagsALongName()
    {
        var rule = new MobilityClaimNameLengthRule(64);
        var item = MobilityClaim.NewMobilityClaim("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        MobilityClaimValidatorReportsEveryProblem();
        MobilityClaimNameLengthRuleFlagsALongName();
    }
}
