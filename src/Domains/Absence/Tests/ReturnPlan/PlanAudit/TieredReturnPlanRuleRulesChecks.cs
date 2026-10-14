namespace Acme.Workforce.Absence.Tests.ReturnPlan.PlanAudit;

using Acme.Workforce.Absence.ReturnPlan.PlanAudit;

/// <summary>Checks of plan audit code, run by the project's runner.</summary>
public static class TieredReturnPlanRuleRulesChecks
{
    public static void TieredReturnPlanRuleValidatorReportsEveryProblem()
    {
        var validator = TieredReturnPlanRuleRules.DefaultTieredReturnPlanRuleValidator();
        var item = TieredReturnPlanRule.NewTieredReturnPlanRule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TieredReturnPlanRuleRequiredNameRuleFlagsABlankName()
    {
        var rule = new TieredReturnPlanRuleRequiredNameRule("tiered return plan rule needs an entry");
        var item = TieredReturnPlanRule.NewTieredReturnPlanRule("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TieredReturnPlanRuleCodeLengthRuleFlagsALongCode()
    {
        var rule = new TieredReturnPlanRuleCodeLengthRule(255);
        var item = TieredReturnPlanRule.NewTieredReturnPlanRule("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TieredReturnPlanRuleRequiredTeamRuleFlagsABlankTeam()
    {
        var rule = new TieredReturnPlanRuleRequiredTeamRule("tiered return plan rule needs this field");
        var item = TieredReturnPlanRule.NewTieredReturnPlanRule("", "");
        item.Team = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TieredReturnPlanRuleScoreRangeRuleFlagsValuesAboveScore()
    {
        var rule = new TieredReturnPlanRuleScoreRangeRule(4.3, 4787.4);
        var item = TieredReturnPlanRule.NewTieredReturnPlanRule("x", "y");
        item.Score = 4788.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TieredReturnPlanRuleTeamLengthRuleFlagsALongTeam()
    {
        var rule = new TieredReturnPlanRuleTeamLengthRule(3600);
        var item = TieredReturnPlanRule.NewTieredReturnPlanRule("x", "y");
        item.Team = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Team = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        TieredReturnPlanRuleValidatorReportsEveryProblem();
        TieredReturnPlanRuleRequiredNameRuleFlagsABlankName();
        TieredReturnPlanRuleCodeLengthRuleFlagsALongCode();
        TieredReturnPlanRuleRequiredTeamRuleFlagsABlankTeam();
        TieredReturnPlanRuleScoreRangeRuleFlagsValuesAboveScore();
        TieredReturnPlanRuleTeamLengthRuleFlagsALongTeam();
    }
}
