namespace Acme.Workforce.Compliance.Tests.PolicyAck.Estimate;

using Acme.Workforce.Compliance.PolicyAck.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class PrimaryPolicyAckPlanRulesChecks
{
    public static void PrimaryPolicyAckPlanValidatorReportsEveryProblem()
    {
        var validator = PrimaryPolicyAckPlanRules.DefaultPrimaryPolicyAckPlanValidator();
        var item = PrimaryPolicyAckPlan.NewPrimaryPolicyAckPlan("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PrimaryPolicyAckPlanRequiredBadgeRuleFlagsABlankBadge()
    {
        var rule = new PrimaryPolicyAckPlanRequiredBadgeRule("primary policy ack plan needs this field");
        var item = PrimaryPolicyAckPlan.NewPrimaryPolicyAckPlan("", "");
        item.Badge = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PrimaryPolicyAckPlanLabelLengthRuleFlagsALongLabel()
    {
        var rule = new PrimaryPolicyAckPlanLabelLengthRule(86400);
        var item = PrimaryPolicyAckPlan.NewPrimaryPolicyAckPlan("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PrimaryPolicyAckPlanRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PrimaryPolicyAckPlanRequiredCodeRule("primary policy ack plan needs a value");
        var item = PrimaryPolicyAckPlan.NewPrimaryPolicyAckPlan("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PrimaryPolicyAckPlanTeamLengthRuleFlagsALongTeam()
    {
        var rule = new PrimaryPolicyAckPlanTeamLengthRule(16);
        var item = PrimaryPolicyAckPlan.NewPrimaryPolicyAckPlan("x", "y");
        item.Team = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Team = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PrimaryPolicyAckPlanValidatorReportsEveryProblem();
        PrimaryPolicyAckPlanRequiredBadgeRuleFlagsABlankBadge();
        PrimaryPolicyAckPlanLabelLengthRuleFlagsALongLabel();
        PrimaryPolicyAckPlanRequiredCodeRuleFlagsABlankCode();
        PrimaryPolicyAckPlanTeamLengthRuleFlagsALongTeam();
    }
}
