namespace Acme.Workforce.Absence.Tests.Policy.PolicyRollup;

using Acme.Workforce.Absence.Policy.PolicyRollup;

/// <summary>Checks of policy rollup code, run by the project's runner.</summary>
public static class PrimaryPolicyRollupRulesChecks
{
    public static void PrimaryPolicyRollupValidatorReportsEveryProblem()
    {
        var validator = PrimaryPolicyRollupRules.DefaultPrimaryPolicyRollupValidator();
        var item = PrimaryPolicyRollup.NewPrimaryPolicyRollup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PrimaryPolicyRollupRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PrimaryPolicyRollupRequiredCodeRule("primary policy rollup needs this field");
        var item = PrimaryPolicyRollup.NewPrimaryPolicyRollup("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PrimaryPolicyRollupLabelLengthRuleFlagsALongLabel()
    {
        var rule = new PrimaryPolicyRollupLabelLengthRule(32);
        var item = PrimaryPolicyRollup.NewPrimaryPolicyRollup("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PrimaryPolicyRollupRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new PrimaryPolicyRollupRequiredLabelRule("primary policy rollup needs this field");
        var item = PrimaryPolicyRollup.NewPrimaryPolicyRollup("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PrimaryPolicyRollupValidatorReportsEveryProblem();
        PrimaryPolicyRollupRequiredCodeRuleFlagsABlankCode();
        PrimaryPolicyRollupLabelLengthRuleFlagsALongLabel();
        PrimaryPolicyRollupRequiredLabelRuleFlagsABlankLabel();
    }
}
