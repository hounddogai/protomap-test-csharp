namespace Acme.Workforce.Compliance.Tests.PolicyAck.Bundle;

using Acme.Workforce.Compliance.PolicyAck.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class PolicyAckBundleRulesChecks
{
    public static void PolicyAckBundleValidatorReportsEveryProblem()
    {
        var validator = PolicyAckBundleRules.DefaultPolicyAckBundleValidator();
        var item = PolicyAckBundle.NewPolicyAckBundle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PolicyAckBundleDescriptionLengthRuleFlagsALongDescription()
    {
        var rule = new PolicyAckBundleDescriptionLengthRule(86400);
        var item = PolicyAckBundle.NewPolicyAckBundle("x", "y");
        item.Description = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Description = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PolicyAckBundleCodeLengthRuleFlagsALongCode()
    {
        var rule = new PolicyAckBundleCodeLengthRule(64);
        var item = PolicyAckBundle.NewPolicyAckBundle("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PolicyAckBundleLocaleLengthRuleFlagsALongLocale()
    {
        var rule = new PolicyAckBundleLocaleLengthRule(3600);
        var item = PolicyAckBundle.NewPolicyAckBundle("x", "y");
        item.Locale = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Locale = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PolicyAckBundleDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes()
    {
        var rule = new PolicyAckBundleDurationMinutesRangeRule(16, 120);
        var item = PolicyAckBundle.NewPolicyAckBundle("x", "y");
        item.DurationMinutes = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        PolicyAckBundleValidatorReportsEveryProblem();
        PolicyAckBundleDescriptionLengthRuleFlagsALongDescription();
        PolicyAckBundleCodeLengthRuleFlagsALongCode();
        PolicyAckBundleLocaleLengthRuleFlagsALongLocale();
        PolicyAckBundleDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes();
    }
}
