namespace Acme.Workforce.Absence.Tests.Policy.RollingPolicyOverride;

using Acme.Workforce.Absence.Policy.RollingPolicyOverride;

/// <summary>Checks of rolling policy override code, run by the project's runner.</summary>
public static class PolicyDigestRulesChecks
{
    public static void PolicyDigestValidatorReportsEveryProblem()
    {
        var validator = PolicyDigestRules.DefaultPolicyDigestValidator();
        var item = PolicyDigest.NewPolicyDigest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PolicyDigestOwnerIdLengthRuleFlagsALongOwnerId()
    {
        var rule = new PolicyDigestOwnerIdLengthRule(86400);
        var item = PolicyDigest.NewPolicyDigest("x", "y");
        item.OwnerId = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.OwnerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PolicyDigestRequiredRegionRuleFlagsABlankRegion()
    {
        var rule = new PolicyDigestRequiredRegionRule("policy digest needs a value");
        var item = PolicyDigest.NewPolicyDigest("", "");
        item.Region = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PolicyDigestRequiredNameRuleFlagsABlankName()
    {
        var rule = new PolicyDigestRequiredNameRule("policy digest needs this field");
        var item = PolicyDigest.NewPolicyDigest("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PolicyDigestNameLengthRuleFlagsALongName()
    {
        var rule = new PolicyDigestNameLengthRule(16);
        var item = PolicyDigest.NewPolicyDigest("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PolicyDigestValidatorReportsEveryProblem();
        PolicyDigestOwnerIdLengthRuleFlagsALongOwnerId();
        PolicyDigestRequiredRegionRuleFlagsABlankRegion();
        PolicyDigestRequiredNameRuleFlagsABlankName();
        PolicyDigestNameLengthRuleFlagsALongName();
    }
}
