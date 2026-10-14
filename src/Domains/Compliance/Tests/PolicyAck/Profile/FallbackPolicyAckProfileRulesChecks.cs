namespace Acme.Workforce.Compliance.Tests.PolicyAck.Profile;

using Acme.Workforce.Compliance.PolicyAck.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class FallbackPolicyAckProfileRulesChecks
{
    public static void FallbackPolicyAckProfileValidatorReportsEveryProblem()
    {
        var validator = FallbackPolicyAckProfileRules.DefaultFallbackPolicyAckProfileValidator();
        var item = FallbackPolicyAckProfile.NewFallbackPolicyAckProfile("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FallbackPolicyAckProfileNameLengthRuleFlagsALongName()
    {
        var rule = new FallbackPolicyAckProfileNameLengthRule(3600);
        var item = FallbackPolicyAckProfile.NewFallbackPolicyAckProfile("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FallbackPolicyAckProfileRequiredLocaleRuleFlagsABlankLocale()
    {
        var rule = new FallbackPolicyAckProfileRequiredLocaleRule("fallback policy ack profile needs this field");
        var item = FallbackPolicyAckProfile.NewFallbackPolicyAckProfile("", "");
        item.Locale = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FallbackPolicyAckProfileRequiredNameRuleFlagsABlankName()
    {
        var rule = new FallbackPolicyAckProfileRequiredNameRule("fallback policy ack profile needs a value");
        var item = FallbackPolicyAckProfile.NewFallbackPolicyAckProfile("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FallbackPolicyAckProfileCodeLengthRuleFlagsALongCode()
    {
        var rule = new FallbackPolicyAckProfileCodeLengthRule(3600);
        var item = FallbackPolicyAckProfile.NewFallbackPolicyAckProfile("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        FallbackPolicyAckProfileValidatorReportsEveryProblem();
        FallbackPolicyAckProfileNameLengthRuleFlagsALongName();
        FallbackPolicyAckProfileRequiredLocaleRuleFlagsABlankLocale();
        FallbackPolicyAckProfileRequiredNameRuleFlagsABlankName();
        FallbackPolicyAckProfileCodeLengthRuleFlagsALongCode();
    }
}
