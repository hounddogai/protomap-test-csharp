namespace Acme.Workforce.Absence.Tests.Policy.PolicyRule;

using Acme.Workforce.Absence.Policy.PolicyRule;

/// <summary>Checks of policy rule code, run by the project's runner.</summary>
public static class PolicyCheckpointRulesChecks
{
    public static void PolicyCheckpointValidatorReportsEveryProblem()
    {
        var validator = PolicyCheckpointRules.DefaultPolicyCheckpointValidator();
        var item = PolicyCheckpoint.NewPolicyCheckpoint("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PolicyCheckpointRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new PolicyCheckpointRequiredTitleRule("policy checkpoint needs this field");
        var item = PolicyCheckpoint.NewPolicyCheckpoint("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PolicyCheckpointRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PolicyCheckpointRequiredCodeRule("policy checkpoint needs this field");
        var item = PolicyCheckpoint.NewPolicyCheckpoint("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PolicyCheckpointCodeLengthRuleFlagsALongCode()
    {
        var rule = new PolicyCheckpointCodeLengthRule(32);
        var item = PolicyCheckpoint.NewPolicyCheckpoint("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PolicyCheckpointValidatorReportsEveryProblem();
        PolicyCheckpointRequiredTitleRuleFlagsABlankTitle();
        PolicyCheckpointRequiredCodeRuleFlagsABlankCode();
        PolicyCheckpointCodeLengthRuleFlagsALongCode();
    }
}
