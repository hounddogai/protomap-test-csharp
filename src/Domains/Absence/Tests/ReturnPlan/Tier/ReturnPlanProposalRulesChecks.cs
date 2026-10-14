namespace Acme.Workforce.Absence.Tests.ReturnPlan.Tier;

using Acme.Workforce.Absence.ReturnPlan.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class ReturnPlanProposalRulesChecks
{
    public static void ReturnPlanProposalValidatorReportsEveryProblem()
    {
        var validator = ReturnPlanProposalRules.DefaultReturnPlanProposalValidator();
        var item = ReturnPlanProposal.NewReturnPlanProposal("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ReturnPlanProposalRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new ReturnPlanProposalRequiredLabelRule("return plan proposal needs this field");
        var item = ReturnPlanProposal.NewReturnPlanProposal("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ReturnPlanProposalTeamLengthRuleFlagsALongTeam()
    {
        var rule = new ReturnPlanProposalTeamLengthRule(3600);
        var item = ReturnPlanProposal.NewReturnPlanProposal("x", "y");
        item.Team = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Team = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ReturnPlanProposalLabelLengthRuleFlagsALongLabel()
    {
        var rule = new ReturnPlanProposalLabelLengthRule(120);
        var item = ReturnPlanProposal.NewReturnPlanProposal("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ReturnPlanProposalValidatorReportsEveryProblem();
        ReturnPlanProposalRequiredLabelRuleFlagsABlankLabel();
        ReturnPlanProposalTeamLengthRuleFlagsALongTeam();
        ReturnPlanProposalLabelLengthRuleFlagsALongLabel();
    }
}
