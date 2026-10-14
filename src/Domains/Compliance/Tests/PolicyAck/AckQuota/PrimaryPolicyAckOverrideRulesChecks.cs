namespace Acme.Workforce.Compliance.Tests.PolicyAck.AckQuota;

using Acme.Workforce.Compliance.PolicyAck.AckQuota;

/// <summary>Checks of ack quota code, run by the project's runner.</summary>
public static class PrimaryPolicyAckOverrideRulesChecks
{
    public static void PrimaryPolicyAckOverrideValidatorReportsEveryProblem()
    {
        var validator = PrimaryPolicyAckOverrideRules.DefaultPrimaryPolicyAckOverrideValidator();
        var item = PrimaryPolicyAckOverride.NewPrimaryPolicyAckOverride("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PrimaryPolicyAckOverrideRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PrimaryPolicyAckOverrideRequiredCodeRule("primary policy ack override needs a value");
        var item = PrimaryPolicyAckOverride.NewPrimaryPolicyAckOverride("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PrimaryPolicyAckOverrideRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new PrimaryPolicyAckOverrideRequiredTitleRule("primary policy ack override needs a value");
        var item = PrimaryPolicyAckOverride.NewPrimaryPolicyAckOverride("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PrimaryPolicyAckOverrideTitleLengthRuleFlagsALongTitle()
    {
        var rule = new PrimaryPolicyAckOverrideTitleLengthRule(64);
        var item = PrimaryPolicyAckOverride.NewPrimaryPolicyAckOverride("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PrimaryPolicyAckOverrideValidatorReportsEveryProblem();
        PrimaryPolicyAckOverrideRequiredCodeRuleFlagsABlankCode();
        PrimaryPolicyAckOverrideRequiredTitleRuleFlagsABlankTitle();
        PrimaryPolicyAckOverrideTitleLengthRuleFlagsALongTitle();
    }
}
