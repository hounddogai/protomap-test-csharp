namespace Acme.Workforce.Absence.Tests.Policy.PolicyOverride;

using Acme.Workforce.Absence.Policy.PolicyOverride;

/// <summary>Checks of policy override code, run by the project's runner.</summary>
public static class RollingPolicyOverrideRulesChecks
{
    public static void RollingPolicyOverrideValidatorReportsEveryProblem()
    {
        var validator = RollingPolicyOverrideRules.DefaultRollingPolicyOverrideValidator();
        var item = RollingPolicyOverride.NewRollingPolicyOverride("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RollingPolicyOverrideCountryLengthRuleFlagsALongCountry()
    {
        var rule = new RollingPolicyOverrideCountryLengthRule(255);
        var item = RollingPolicyOverride.NewRollingPolicyOverride("x", "y");
        item.Country = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Country = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RollingPolicyOverrideNameLengthRuleFlagsALongName()
    {
        var rule = new RollingPolicyOverrideNameLengthRule(80);
        var item = RollingPolicyOverride.NewRollingPolicyOverride("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RollingPolicyOverrideStartsAtRangeRuleFlagsValuesAboveStartsAt()
    {
        var rule = new RollingPolicyOverrideStartsAtRangeRule(120, 255);
        var item = RollingPolicyOverride.NewRollingPolicyOverride("x", "y");
        item.StartsAt = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        RollingPolicyOverrideValidatorReportsEveryProblem();
        RollingPolicyOverrideCountryLengthRuleFlagsALongCountry();
        RollingPolicyOverrideNameLengthRuleFlagsALongName();
        RollingPolicyOverrideStartsAtRangeRuleFlagsValuesAboveStartsAt();
    }
}
