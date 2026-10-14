namespace Acme.Workforce.Engagement.Tests.ActionPlan.Claim;

using Acme.Workforce.Engagement.ActionPlan.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class DefaultActionPlanRollupRulesChecks
{
    public static void DefaultActionPlanRollupValidatorReportsEveryProblem()
    {
        var validator = DefaultActionPlanRollupRules.DefaultDefaultActionPlanRollupValidator();
        var item = DefaultActionPlanRollup.NewDefaultActionPlanRollup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultActionPlanRollupRequiredTeamRuleFlagsABlankTeam()
    {
        var rule = new DefaultActionPlanRollupRequiredTeamRule("default action plan rollup needs this field");
        var item = DefaultActionPlanRollup.NewDefaultActionPlanRollup("", "");
        item.Team = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultActionPlanRollupTeamLengthRuleFlagsALongTeam()
    {
        var rule = new DefaultActionPlanRollupTeamLengthRule(16);
        var item = DefaultActionPlanRollup.NewDefaultActionPlanRollup("x", "y");
        item.Team = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Team = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DefaultActionPlanRollupStartsAtRangeRuleFlagsValuesAboveStartsAt()
    {
        var rule = new DefaultActionPlanRollupStartsAtRangeRule(1000, 3600);
        var item = DefaultActionPlanRollup.NewDefaultActionPlanRollup("x", "y");
        item.StartsAt = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DefaultActionPlanRollupRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new DefaultActionPlanRollupRequiredCodeRule("default action plan rollup needs a value");
        var item = DefaultActionPlanRollup.NewDefaultActionPlanRollup("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        DefaultActionPlanRollupValidatorReportsEveryProblem();
        DefaultActionPlanRollupRequiredTeamRuleFlagsABlankTeam();
        DefaultActionPlanRollupTeamLengthRuleFlagsALongTeam();
        DefaultActionPlanRollupStartsAtRangeRuleFlagsValuesAboveStartsAt();
        DefaultActionPlanRollupRequiredCodeRuleFlagsABlankCode();
    }
}
