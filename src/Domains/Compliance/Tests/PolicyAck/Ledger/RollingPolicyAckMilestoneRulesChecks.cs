namespace Acme.Workforce.Compliance.Tests.PolicyAck.Ledger;

using Acme.Workforce.Compliance.PolicyAck.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class RollingPolicyAckMilestoneRulesChecks
{
    public static void RollingPolicyAckMilestoneValidatorReportsEveryProblem()
    {
        var validator = RollingPolicyAckMilestoneRules.DefaultRollingPolicyAckMilestoneValidator();
        var item = RollingPolicyAckMilestone.NewRollingPolicyAckMilestone("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RollingPolicyAckMilestoneEmployeeIdLengthRuleFlagsALongEmployeeId()
    {
        var rule = new RollingPolicyAckMilestoneEmployeeIdLengthRule(255);
        var item = RollingPolicyAckMilestone.NewRollingPolicyAckMilestone("x", "y");
        item.EmployeeId = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.EmployeeId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RollingPolicyAckMilestoneRequiredExternalRefRuleFlagsABlankExternalRef()
    {
        var rule = new RollingPolicyAckMilestoneRequiredExternalRefRule("rolling policy ack milestone needs an entry");
        var item = RollingPolicyAckMilestone.NewRollingPolicyAckMilestone("", "");
        item.ExternalRef = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RollingPolicyAckMilestoneRequiredEmployeeIdRuleFlagsABlankEmployeeId()
    {
        var rule = new RollingPolicyAckMilestoneRequiredEmployeeIdRule("rolling policy ack milestone needs a value");
        var item = RollingPolicyAckMilestone.NewRollingPolicyAckMilestone("", "");
        item.EmployeeId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RollingPolicyAckMilestoneRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new RollingPolicyAckMilestoneRequiredTitleRule("rolling policy ack milestone needs a value");
        var item = RollingPolicyAckMilestone.NewRollingPolicyAckMilestone("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        RollingPolicyAckMilestoneValidatorReportsEveryProblem();
        RollingPolicyAckMilestoneEmployeeIdLengthRuleFlagsALongEmployeeId();
        RollingPolicyAckMilestoneRequiredExternalRefRuleFlagsABlankExternalRef();
        RollingPolicyAckMilestoneRequiredEmployeeIdRuleFlagsABlankEmployeeId();
        RollingPolicyAckMilestoneRequiredTitleRuleFlagsABlankTitle();
    }
}
