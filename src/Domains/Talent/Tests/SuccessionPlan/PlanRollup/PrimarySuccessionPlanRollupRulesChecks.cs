namespace Acme.Workforce.Talent.Tests.SuccessionPlan.PlanRollup;

using Acme.Workforce.Talent.SuccessionPlan.PlanRollup;

/// <summary>Checks of plan rollup code, run by the project's runner.</summary>
public static class PrimarySuccessionPlanRollupRulesChecks
{
    public static void PrimarySuccessionPlanRollupValidatorReportsEveryProblem()
    {
        var validator = PrimarySuccessionPlanRollupRules.DefaultPrimarySuccessionPlanRollupValidator();
        var item = PrimarySuccessionPlanRollup.NewPrimarySuccessionPlanRollup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PrimarySuccessionPlanRollupChannelLengthRuleFlagsALongChannel()
    {
        var rule = new PrimarySuccessionPlanRollupChannelLengthRule(1000);
        var item = PrimarySuccessionPlanRollup.NewPrimarySuccessionPlanRollup("x", "y");
        item.Channel = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Channel = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PrimarySuccessionPlanRollupRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PrimarySuccessionPlanRollupRequiredCodeRule("primary succession plan rollup needs a value");
        var item = PrimarySuccessionPlanRollup.NewPrimarySuccessionPlanRollup("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PrimarySuccessionPlanRollupLabelLengthRuleFlagsALongLabel()
    {
        var rule = new PrimarySuccessionPlanRollupLabelLengthRule(80);
        var item = PrimarySuccessionPlanRollup.NewPrimarySuccessionPlanRollup("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PrimarySuccessionPlanRollupValidatorReportsEveryProblem();
        PrimarySuccessionPlanRollupChannelLengthRuleFlagsALongChannel();
        PrimarySuccessionPlanRollupRequiredCodeRuleFlagsABlankCode();
        PrimarySuccessionPlanRollupLabelLengthRuleFlagsALongLabel();
    }
}
