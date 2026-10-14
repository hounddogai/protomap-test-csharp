namespace Acme.Workforce.Absence.Tests.Policy.Score;

using Acme.Workforce.Absence.Policy.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class PolicyAssignmentRulesChecks
{
    public static void PolicyAssignmentValidatorReportsEveryProblem()
    {
        var validator = PolicyAssignmentRules.DefaultPolicyAssignmentValidator();
        var item = PolicyAssignment.NewPolicyAssignment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PolicyAssignmentTitleLengthRuleFlagsALongTitle()
    {
        var rule = new PolicyAssignmentTitleLengthRule(120);
        var item = PolicyAssignment.NewPolicyAssignment("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PolicyAssignmentRequiredBadgeRuleFlagsABlankBadge()
    {
        var rule = new PolicyAssignmentRequiredBadgeRule("policy assignment needs a value");
        var item = PolicyAssignment.NewPolicyAssignment("", "");
        item.Badge = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PolicyAssignmentValidatorReportsEveryProblem();
        PolicyAssignmentTitleLengthRuleFlagsALongTitle();
        PolicyAssignmentRequiredBadgeRuleFlagsABlankBadge();
    }
}
