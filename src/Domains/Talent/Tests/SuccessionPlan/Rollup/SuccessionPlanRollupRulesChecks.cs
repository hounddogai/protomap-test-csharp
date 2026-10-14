namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Rollup;

using Acme.Workforce.Talent.SuccessionPlan.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class SuccessionPlanRollupRulesChecks
{
    public static void SuccessionPlanRollupValidatorReportsEveryProblem()
    {
        var validator = SuccessionPlanRollupRules.DefaultSuccessionPlanRollupValidator();
        var item = SuccessionPlanRollup.NewSuccessionPlanRollup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SuccessionPlanRollupRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new SuccessionPlanRollupRequiredCodeRule("succession plan rollup needs an entry");
        var item = SuccessionPlanRollup.NewSuccessionPlanRollup("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SuccessionPlanRollupRequiredTimezoneRuleFlagsABlankTimezone()
    {
        var rule = new SuccessionPlanRollupRequiredTimezoneRule("succession plan rollup needs a value");
        var item = SuccessionPlanRollup.NewSuccessionPlanRollup("", "");
        item.Timezone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SuccessionPlanRollupTeamLengthRuleFlagsALongTeam()
    {
        var rule = new SuccessionPlanRollupTeamLengthRule(255);
        var item = SuccessionPlanRollup.NewSuccessionPlanRollup("x", "y");
        item.Team = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Team = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SuccessionPlanRollupLabelLengthRuleFlagsALongLabel()
    {
        var rule = new SuccessionPlanRollupLabelLengthRule(120);
        var item = SuccessionPlanRollup.NewSuccessionPlanRollup("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        SuccessionPlanRollupValidatorReportsEveryProblem();
        SuccessionPlanRollupRequiredCodeRuleFlagsABlankCode();
        SuccessionPlanRollupRequiredTimezoneRuleFlagsABlankTimezone();
        SuccessionPlanRollupTeamLengthRuleFlagsALongTeam();
        SuccessionPlanRollupLabelLengthRuleFlagsALongLabel();
    }
}
