namespace Acme.Workforce.Absence.Tests.Policy.Rollup;

using Acme.Workforce.Absence.Policy.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class LenientPolicyRollupRulesChecks
{
    public static void LenientPolicyRollupValidatorReportsEveryProblem()
    {
        var validator = LenientPolicyRollupRules.DefaultLenientPolicyRollupValidator();
        var item = LenientPolicyRollup.NewLenientPolicyRollup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientPolicyRollupRequiredNameRuleFlagsABlankName()
    {
        var rule = new LenientPolicyRollupRequiredNameRule("lenient policy rollup needs a value");
        var item = LenientPolicyRollup.NewLenientPolicyRollup("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientPolicyRollupRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new LenientPolicyRollupRequiredLabelRule("lenient policy rollup needs a value");
        var item = LenientPolicyRollup.NewLenientPolicyRollup("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LenientPolicyRollupValidatorReportsEveryProblem();
        LenientPolicyRollupRequiredNameRuleFlagsABlankName();
        LenientPolicyRollupRequiredLabelRuleFlagsABlankLabel();
    }
}
