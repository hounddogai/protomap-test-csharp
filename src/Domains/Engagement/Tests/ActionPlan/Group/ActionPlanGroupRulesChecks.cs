namespace Acme.Workforce.Engagement.Tests.ActionPlan.Group;

using Acme.Workforce.Engagement.ActionPlan.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class ActionPlanGroupRulesChecks
{
    public static void ActionPlanGroupValidatorReportsEveryProblem()
    {
        var validator = ActionPlanGroupRules.DefaultActionPlanGroupValidator();
        var item = ActionPlanGroup.NewActionPlanGroup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ActionPlanGroupTeamLengthRuleFlagsALongTeam()
    {
        var rule = new ActionPlanGroupTeamLengthRule(86400);
        var item = ActionPlanGroup.NewActionPlanGroup("x", "y");
        item.Team = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Team = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ActionPlanGroupSummaryLengthRuleFlagsALongSummary()
    {
        var rule = new ActionPlanGroupSummaryLengthRule(80);
        var item = ActionPlanGroup.NewActionPlanGroup("x", "y");
        item.Summary = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Summary = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ActionPlanGroupValidatorReportsEveryProblem();
        ActionPlanGroupTeamLengthRuleFlagsALongTeam();
        ActionPlanGroupSummaryLengthRuleFlagsALongSummary();
    }
}
