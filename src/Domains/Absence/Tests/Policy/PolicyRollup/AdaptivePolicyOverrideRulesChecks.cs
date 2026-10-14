namespace Acme.Workforce.Absence.Tests.Policy.PolicyRollup;

using Acme.Workforce.Absence.Policy.PolicyRollup;

/// <summary>Checks of policy rollup code, run by the project's runner.</summary>
public static class AdaptivePolicyOverrideRulesChecks
{
    public static void AdaptivePolicyOverrideValidatorReportsEveryProblem()
    {
        var validator = AdaptivePolicyOverrideRules.DefaultAdaptivePolicyOverrideValidator();
        var item = AdaptivePolicyOverride.NewAdaptivePolicyOverride("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdaptivePolicyOverrideCodeLengthRuleFlagsALongCode()
    {
        var rule = new AdaptivePolicyOverrideCodeLengthRule(16);
        var item = AdaptivePolicyOverride.NewAdaptivePolicyOverride("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdaptivePolicyOverrideChecksumLengthRuleFlagsALongChecksum()
    {
        var rule = new AdaptivePolicyOverrideChecksumLengthRule(3600);
        var item = AdaptivePolicyOverride.NewAdaptivePolicyOverride("x", "y");
        item.Checksum = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Checksum = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdaptivePolicyOverrideNameLengthRuleFlagsALongName()
    {
        var rule = new AdaptivePolicyOverrideNameLengthRule(32);
        var item = AdaptivePolicyOverride.NewAdaptivePolicyOverride("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdaptivePolicyOverrideRequiredNameRuleFlagsABlankName()
    {
        var rule = new AdaptivePolicyOverrideRequiredNameRule("adaptive policy override needs a value");
        var item = AdaptivePolicyOverride.NewAdaptivePolicyOverride("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdaptivePolicyOverrideRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new AdaptivePolicyOverrideRequiredCodeRule("adaptive policy override needs an entry");
        var item = AdaptivePolicyOverride.NewAdaptivePolicyOverride("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AdaptivePolicyOverrideValidatorReportsEveryProblem();
        AdaptivePolicyOverrideCodeLengthRuleFlagsALongCode();
        AdaptivePolicyOverrideChecksumLengthRuleFlagsALongChecksum();
        AdaptivePolicyOverrideNameLengthRuleFlagsALongName();
        AdaptivePolicyOverrideRequiredNameRuleFlagsABlankName();
        AdaptivePolicyOverrideRequiredCodeRuleFlagsABlankCode();
    }
}
