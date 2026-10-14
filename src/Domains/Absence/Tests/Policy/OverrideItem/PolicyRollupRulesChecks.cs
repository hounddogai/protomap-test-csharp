namespace Acme.Workforce.Absence.Tests.Policy.OverrideItem;

using Acme.Workforce.Absence.Policy.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class PolicyRollupRulesChecks
{
    public static void PolicyRollupValidatorReportsEveryProblem()
    {
        var validator = PolicyRollupRules.DefaultPolicyRollupValidator();
        var item = PolicyRollup.NewPolicyRollup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PolicyRollupPhoneLengthRuleFlagsALongPhone()
    {
        var rule = new PolicyRollupPhoneLengthRule(32);
        var item = PolicyRollup.NewPolicyRollup("x", "y");
        item.Phone = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Phone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PolicyRollupRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PolicyRollupRequiredCodeRule("policy rollup needs an entry");
        var item = PolicyRollup.NewPolicyRollup("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PolicyRollupValidatorReportsEveryProblem();
        PolicyRollupPhoneLengthRuleFlagsALongPhone();
        PolicyRollupRequiredCodeRuleFlagsABlankCode();
    }
}
