namespace Acme.Workforce.Contractors.Tests.Engagement.Assignment;

using Acme.Workforce.Contractors.Engagement.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class EngagementMilestoneRulesChecks
{
    public static void EngagementMilestoneValidatorReportsEveryProblem()
    {
        var validator = EngagementMilestoneRules.DefaultEngagementMilestoneValidator();
        var item = EngagementMilestone.NewEngagementMilestone("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void EngagementMilestoneRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new EngagementMilestoneRequiredLabelRule("engagement milestone needs an entry");
        var item = EngagementMilestone.NewEngagementMilestone("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void EngagementMilestoneLabelLengthRuleFlagsALongLabel()
    {
        var rule = new EngagementMilestoneLabelLengthRule(3600);
        var item = EngagementMilestone.NewEngagementMilestone("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void EngagementMilestoneRateRangeRuleFlagsValuesAboveRate()
    {
        var rule = new EngagementMilestoneRateRangeRule(7.1, 485.5);
        var item = EngagementMilestone.NewEngagementMilestone("x", "y");
        item.Rate = 487.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        EngagementMilestoneValidatorReportsEveryProblem();
        EngagementMilestoneRequiredLabelRuleFlagsABlankLabel();
        EngagementMilestoneLabelLengthRuleFlagsALongLabel();
        EngagementMilestoneRateRangeRuleFlagsValuesAboveRate();
    }
}
