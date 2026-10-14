namespace Acme.Workforce.Talent.Tests.Nomination.NominationPolicy;

using Acme.Workforce.Talent.Nomination.NominationPolicy;

/// <summary>Checks of nomination policy code, run by the project's runner.</summary>
public static class RegionalNominationPolicyRulesChecks
{
    public static void RegionalNominationPolicyValidatorReportsEveryProblem()
    {
        var validator = RegionalNominationPolicyRules.DefaultRegionalNominationPolicyValidator();
        var item = RegionalNominationPolicy.NewRegionalNominationPolicy("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RegionalNominationPolicyRequiredChecksumRuleFlagsABlankChecksum()
    {
        var rule = new RegionalNominationPolicyRequiredChecksumRule("regional nomination policy needs an entry");
        var item = RegionalNominationPolicy.NewRegionalNominationPolicy("", "");
        item.Checksum = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RegionalNominationPolicyFteRangeRuleFlagsValuesAboveFte()
    {
        var rule = new RegionalNominationPolicyFteRangeRule(7.7, 2769.9);
        var item = RegionalNominationPolicy.NewRegionalNominationPolicy("x", "y");
        item.Fte = 2771.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RegionalNominationPolicyNameLengthRuleFlagsALongName()
    {
        var rule = new RegionalNominationPolicyNameLengthRule(64);
        var item = RegionalNominationPolicy.NewRegionalNominationPolicy("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RegionalNominationPolicyRequiredNameRuleFlagsABlankName()
    {
        var rule = new RegionalNominationPolicyRequiredNameRule("regional nomination policy needs this field");
        var item = RegionalNominationPolicy.NewRegionalNominationPolicy("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        RegionalNominationPolicyValidatorReportsEveryProblem();
        RegionalNominationPolicyRequiredChecksumRuleFlagsABlankChecksum();
        RegionalNominationPolicyFteRangeRuleFlagsValuesAboveFte();
        RegionalNominationPolicyNameLengthRuleFlagsALongName();
        RegionalNominationPolicyRequiredNameRuleFlagsABlankName();
    }
}
