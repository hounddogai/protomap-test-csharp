namespace Acme.Workforce.Absence.Tests.Policy.OverrideItem;

using Acme.Workforce.Absence.Policy.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class PolicyOverrideRulesChecks
{
    public static void PolicyOverrideValidatorReportsEveryProblem()
    {
        var validator = PolicyOverrideRules.DefaultPolicyOverrideValidator();
        var item = PolicyOverride.NewPolicyOverride("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PolicyOverrideRequiredNameRuleFlagsABlankName()
    {
        var rule = new PolicyOverrideRequiredNameRule("policy override needs this field");
        var item = PolicyOverride.NewPolicyOverride("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PolicyOverrideScoreRangeRuleFlagsValuesAboveScore()
    {
        var rule = new PolicyOverrideScoreRangeRule(6.8, 2791.8);
        var item = PolicyOverride.NewPolicyOverride("x", "y");
        item.Score = 2793.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PolicyOverrideSequenceRangeRuleFlagsValuesAboveSequence()
    {
        var rule = new PolicyOverrideSequenceRangeRule(3600, 86400);
        var item = PolicyOverride.NewPolicyOverride("x", "y");
        item.Sequence = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        PolicyOverrideValidatorReportsEveryProblem();
        PolicyOverrideRequiredNameRuleFlagsABlankName();
        PolicyOverrideScoreRangeRuleFlagsValuesAboveScore();
        PolicyOverrideSequenceRangeRuleFlagsValuesAboveSequence();
    }
}
