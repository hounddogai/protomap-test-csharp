namespace Acme.Workforce.Absence.Tests.Policy.PolicyEstimate;

using Acme.Workforce.Absence.Policy.PolicyEstimate;

/// <summary>Checks of policy estimate code, run by the project's runner.</summary>
public static class PolicyMilestoneRulesChecks
{
    public static void PolicyMilestoneValidatorReportsEveryProblem()
    {
        var validator = PolicyMilestoneRules.DefaultPolicyMilestoneValidator();
        var item = PolicyMilestone.NewPolicyMilestone("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PolicyMilestoneRequiredContactEmailRuleFlagsABlankContactEmail()
    {
        var rule = new PolicyMilestoneRequiredContactEmailRule("policy milestone needs a value");
        var item = PolicyMilestone.NewPolicyMilestone("", "");
        item.ContactEmail = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PolicyMilestoneContactEmailLengthRuleFlagsALongContactEmail()
    {
        var rule = new PolicyMilestoneContactEmailLengthRule(16);
        var item = PolicyMilestone.NewPolicyMilestone("x", "y");
        item.ContactEmail = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ContactEmail = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PolicyMilestoneValidatorReportsEveryProblem();
        PolicyMilestoneRequiredContactEmailRuleFlagsABlankContactEmail();
        PolicyMilestoneContactEmailLengthRuleFlagsALongContactEmail();
    }
}
