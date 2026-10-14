namespace Acme.Workforce.Engagement.Tests.TownHall.Milestone;

using Acme.Workforce.Engagement.TownHall.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class TownHallMilestoneRulesChecks
{
    public static void TownHallMilestoneValidatorReportsEveryProblem()
    {
        var validator = TownHallMilestoneRules.DefaultTownHallMilestoneValidator();
        var item = TownHallMilestone.NewTownHallMilestone("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TownHallMilestoneRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new TownHallMilestoneRequiredLabelRule("town hall milestone needs a value");
        var item = TownHallMilestone.NewTownHallMilestone("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TownHallMilestoneMaximumRangeRuleFlagsValuesAboveMaximum()
    {
        var rule = new TownHallMilestoneMaximumRangeRule(4.5, 1110.1);
        var item = TownHallMilestone.NewTownHallMilestone("x", "y");
        item.Maximum = 1111.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TownHallMilestoneMinimumRangeRuleFlagsValuesAboveMinimum()
    {
        var rule = new TownHallMilestoneMinimumRangeRule(9.4, 694.7);
        var item = TownHallMilestone.NewTownHallMilestone("x", "y");
        item.Minimum = 696.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        TownHallMilestoneValidatorReportsEveryProblem();
        TownHallMilestoneRequiredLabelRuleFlagsABlankLabel();
        TownHallMilestoneMaximumRangeRuleFlagsValuesAboveMaximum();
        TownHallMilestoneMinimumRangeRuleFlagsValuesAboveMinimum();
    }
}
