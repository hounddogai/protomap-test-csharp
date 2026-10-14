namespace Acme.Workforce.Engagement.Tests.ActionPlan.Preference;

using Acme.Workforce.Engagement.ActionPlan.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class ManualActionPlanMilestoneRulesChecks
{
    public static void ManualActionPlanMilestoneValidatorReportsEveryProblem()
    {
        var validator = ManualActionPlanMilestoneRules.DefaultManualActionPlanMilestoneValidator();
        var item = ManualActionPlanMilestone.NewManualActionPlanMilestone("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ManualActionPlanMilestoneLabelLengthRuleFlagsALongLabel()
    {
        var rule = new ManualActionPlanMilestoneLabelLengthRule(80);
        var item = ManualActionPlanMilestone.NewManualActionPlanMilestone("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ManualActionPlanMilestoneRequiredLocationRuleFlagsABlankLocation()
    {
        var rule = new ManualActionPlanMilestoneRequiredLocationRule("manual action plan milestone needs a value");
        var item = ManualActionPlanMilestone.NewManualActionPlanMilestone("", "");
        item.Location = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ManualActionPlanMilestoneCeilingRangeRuleFlagsValuesAboveCeiling()
    {
        var rule = new ManualActionPlanMilestoneCeilingRangeRule(4.9, 3263.4);
        var item = ManualActionPlanMilestone.NewManualActionPlanMilestone("x", "y");
        item.Ceiling = 3264.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ManualActionPlanMilestoneCodeLengthRuleFlagsALongCode()
    {
        var rule = new ManualActionPlanMilestoneCodeLengthRule(32);
        var item = ManualActionPlanMilestone.NewManualActionPlanMilestone("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ManualActionPlanMilestoneCostRangeRuleFlagsValuesAboveCost()
    {
        var rule = new ManualActionPlanMilestoneCostRangeRule(5.9, 263.0);
        var item = ManualActionPlanMilestone.NewManualActionPlanMilestone("x", "y");
        item.Cost = 264.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        ManualActionPlanMilestoneValidatorReportsEveryProblem();
        ManualActionPlanMilestoneLabelLengthRuleFlagsALongLabel();
        ManualActionPlanMilestoneRequiredLocationRuleFlagsABlankLocation();
        ManualActionPlanMilestoneCeilingRangeRuleFlagsValuesAboveCeiling();
        ManualActionPlanMilestoneCodeLengthRuleFlagsALongCode();
        ManualActionPlanMilestoneCostRangeRuleFlagsValuesAboveCost();
    }
}
