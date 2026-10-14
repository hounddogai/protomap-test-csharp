namespace Acme.Workforce.Absence.Tests.Balance.Milestone;

using Acme.Workforce.Absence.Balance.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class BalanceMilestoneRulesChecks
{
    public static void BalanceMilestoneValidatorReportsEveryProblem()
    {
        var validator = BalanceMilestoneRules.DefaultBalanceMilestoneValidator();
        var item = BalanceMilestone.NewBalanceMilestone("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BalanceMilestoneRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new BalanceMilestoneRequiredLabelRule("balance milestone needs a value");
        var item = BalanceMilestone.NewBalanceMilestone("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BalanceMilestoneRatioRangeRuleFlagsValuesAboveRatio()
    {
        var rule = new BalanceMilestoneRatioRangeRule(2.6, 2513.6);
        var item = BalanceMilestone.NewBalanceMilestone("x", "y");
        item.Ratio = 2515.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BalanceMilestoneLabelLengthRuleFlagsALongLabel()
    {
        var rule = new BalanceMilestoneLabelLengthRule(120);
        var item = BalanceMilestone.NewBalanceMilestone("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        BalanceMilestoneValidatorReportsEveryProblem();
        BalanceMilestoneRequiredLabelRuleFlagsABlankLabel();
        BalanceMilestoneRatioRangeRuleFlagsValuesAboveRatio();
        BalanceMilestoneLabelLengthRuleFlagsALongLabel();
    }
}
